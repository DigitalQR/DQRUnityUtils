using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ShaderGraph;
#endif

namespace DQR.Voxel.Model
{
	[System.Serializable]
	public struct VoxelMaterialLayoutDesc : ISerializationCallbackReceiver
	{
		public enum DataLayout
		{
			Float1,
			Float2,
			Float3,
			Float4,
			Colour,
		}

		public string Identifier;
		public DataLayout DataFormat;

		public float MinRange;
		public float MaxRange;

		public bool ShouldClampRange
		{
			get => !(MinRange == 0.0f && MaxRange == 0.0f);
		}

		public float ProcessValue(float inV)
		{
			return ShouldClampRange ? Mathf.Clamp(inV, MinRange, MaxRange) : inV;
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
		}
	}

	public static class VoxelMaterialLayoutDescExt
	{
		public static int GetChannelCount(this VoxelMaterialLayoutDesc.DataLayout layout)
		{
			switch (layout)
			{
				case VoxelMaterialLayoutDesc.DataLayout.Float1:
					return 1;
				case VoxelMaterialLayoutDesc.DataLayout.Float2:
					return 2;
				case VoxelMaterialLayoutDesc.DataLayout.Float3:
					return 3;
				case VoxelMaterialLayoutDesc.DataLayout.Float4:
				case VoxelMaterialLayoutDesc.DataLayout.Colour:
					return 4;
			}

			return 0;
		}
	}

	[CreateAssetMenu(menuName = "DQR/Voxel/New Voxel Material Layout")]
	[System.Serializable]
	public class VoxelMaterialLayout : ScriptableObject, ISerializationCallbackReceiver
	{
		private const string c_ShaderGraphTemplateAssetPath = "Assets\\DQR\\Library\\Voxel\\Shaders\\TEMPLATE_GetVoxelMaterial.shadersubgraph";

		[SerializeField]
		private VoxelMaterialLayoutDesc[] m_Layouts = null;
		private int m_ChannelCount = 0;

		[SerializeField]
		private bool m_GenerateShaderCode = true;

		public int LayoutCount
		{
			get => m_Layouts.Length;
		}

		public int ChannelCount
		{
			get => m_ChannelCount;
		}

		public VoxelMaterialLayoutDesc GetLayoutDesc(int index)
		{
			return m_Layouts[index];
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			m_ChannelCount = 0;
			if (m_Layouts != null)
			{
				foreach (var layout in m_Layouts)
					m_ChannelCount += layout.DataFormat.GetChannelCount();
			}
		}

		internal void RegenerateExports()
		{
			string sourcePath = AssetDatabase.GetAssetPath(this);
			string sourceDir = Path.GetDirectoryName(sourcePath);
			string assetName = Path.GetFileNameWithoutExtension(sourcePath);
			
			string shaderGraphPath = Path.Combine(sourceDir, "Generated", "GetVoxelMaterial_" + assetName + ".shadersubgraph");
			
			if (m_GenerateShaderCode)
			{
				string sourceCode = GenerateShaderCode(assetName);

				var shaderGraph = AssetDatabase.LoadAssetAtPath<Object>(shaderGraphPath);
				if (shaderGraph == null)
				{
					DQRUtils.EnsureAssetDirectoryExists(Path.GetDirectoryName(shaderGraphPath));
					AssetDatabase.CopyAsset(c_ShaderGraphTemplateAssetPath, shaderGraphPath);
				}

				// Manually tweak the the asset file
				string sourceAsset = File.ReadAllText(shaderGraphPath);
				string generatedAsset = FixupTemplate(assetName, File.ReadAllText(c_ShaderGraphTemplateAssetPath));

				if (sourceAsset != generatedAsset)
				{
					File.WriteAllText(shaderGraphPath, generatedAsset);
					AssetDatabase.ImportAsset(shaderGraphPath);
				}
			}
		}

		private const int c_MaxSupportedTemplateParams = 16;

		private string FixupTemplate(string assetName, string source)
		{
			string splitKey = "\n}\n";
			string[] parts = source.Replace("\r\n", "\n").Replace(splitKey, "£").Split('£');
			List<string> outputParts = new List<string>();

			// Fix up inputs
			List<string> removedGuids = new List<string>();

			foreach (var it in parts)
			{
				string part = it;

				for (int i = 0; i < c_MaxSupportedTemplateParams; ++i)
				{
					if (part.Contains($"\"m_ShaderOutputName\": \"_output_{i}_\""))
					{
						// Store guid
						int guidIndex = part.IndexOf("\"m_ObjectId\":");
						string guidString = "";
						Assert.Message(guidIndex != 0, "Failed to locate m_ObjectId");

						if (guidIndex != 0)
						{
							guidString = part.Substring(guidIndex).Split(':')[1].Split(',')[0].Trim();
						}	

						if (i < m_Layouts.Length)
						{
							var curr = m_Layouts[i];
							part = part
								.Replace($"_output_{i}_", curr.Identifier)
								.Replace("UnityEditor.ShaderGraph.Vector1MaterialSlot", $"UnityEditor.ShaderGraph.Vector{curr.DataFormat.GetChannelCount()}MaterialSlot");
						}
						else
						{
							// placeholder is out of range
							part = null;
							removedGuids.Add(guidString);
							break;
						}
					}
				}

				if(part != null)
					outputParts.Add(part);
			}
			
			for (int i = 0; i < outputParts.Count; ++i)
			{
				string part = outputParts[i];
				
				// Update slot references for any knowingly removed guids
				if (part.Contains("m_Slots"))
				{
					// Replace main function body
					part = part.Replace("_inner_func_", GenerateShaderCode(assetName).Replace("\r\n", "\n").Replace("\n", "\\n"));

					// Remove slots with invalid guids
					string startMarker = "\"m_Slots\": [";
					int startIndex = part.IndexOf(startMarker);
					Assert.Condition(startIndex != 0);

					startIndex += startMarker.Length;

					int endIndex = part.IndexOf(']', startIndex);
					Assert.Condition(endIndex != 0);

					string slotsArrayRaw = part.Substring(startIndex, endIndex - startIndex);
					string[] slotsArray = slotsArrayRaw.Split(',');

					List<string> outputSlots = new List<string>();

					foreach (var slot in slotsArray)
					{
						bool isValid = !removedGuids.Where((g) => slot.Contains(g)).Any();

						if (isValid)
							outputSlots.Add(slot);
					}

					string newSlotsArray = string.Join(",", outputSlots);
					part = part.Substring(0, startIndex) + newSlotsArray + part.Substring(endIndex);
				}
				
				outputParts[i] = part;
			}

			return string.Join(splitKey, outputParts).Replace("\n", "\r\n");
		}
				
		private string GenerateShaderCode(string assetName)
		{
			string GetFloatType(int count)
			{
				return count == 1 ? "float" : ("float" + count);
			}

			string GetComponent(int i)
			{
				switch (i % 4)
				{
					case 0:
						return "x";
					case 1:
						return "y";
					case 2:
						return "z";
					default:
						return "w";
				}
			}

			var outputVars = m_Layouts.Select((l) => 
			{
				int channel = l.DataFormat.GetChannelCount();
				return $"out {GetFloatType(channel)} out{l.Identifier}";
			});

			string inner = "";

			for (int i = 0; i < ChannelCount; i += 4)
			{
				inner += $"float4 mat{i/4} = mappingTex.Load(float3(matId, {i/4}, 0));\r\n";
			}

			int offset = 0;
			foreach (var l in m_Layouts)
			{
				int count = l.DataFormat.GetChannelCount();
				inner += $"out{l.Identifier} = {GetFloatType(count)}(";

				for (int i = 0; i < count; ++i, ++offset)
				{
					if (i != 0)
						inner += ", ";

					int texId = offset / 4;
					inner += $"mat{texId}." + GetComponent(offset);
				}

				inner += ");\r\n";
			}

			string assetToken = assetName.ToUpper().Replace(" ", "");

			return $@"
#ifndef VOXELLAYOUT_{assetToken}_INCLUDED
#define VOXELLAYOUT_{assetToken}_INCLUDED

void GetVoxelMaterial_{assetToken}_float(Texture2D mappingTex, float4 uv1, {string.Join(", ", outputVars)})
{{
float matId = uv1.x;
{inner}
}}
#endif
";
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(VoxelMaterialLayout))]
	//[CanEditMultipleObjects]
	public class VoxelMaterialLayoutEditor : Editor
	{
		private SerializedProperty m_Prop_Layout;
		private SerializedProperty m_Prop_GenerateShaderCode;

		private void OnEnable()
		{
			m_Prop_Layout = serializedObject.FindProperty("m_Layouts");
			m_Prop_GenerateShaderCode = serializedObject.FindProperty("m_GenerateShaderCode");
		}

		private void OnDisable()
		{
			(serializedObject.targetObject as VoxelMaterialLayout).RegenerateExports();
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.PropertyField(m_Prop_GenerateShaderCode);
			EditorGUILayout.PropertyField(m_Prop_Layout);

			if (serializedObject.ApplyModifiedProperties())
			{
			}
		}
	}
#endif
}