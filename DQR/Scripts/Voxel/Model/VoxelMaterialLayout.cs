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

			string shaderPath = Path.Combine(sourceDir, assetName + ".hlsl");
			TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(shaderPath);

			if (m_GenerateShaderCode)
			{
				string sourceCode = GenerateShaderCode(assetName);

				if (textAsset == null)
				{
					textAsset = new TextAsset(sourceCode);
					AssetDatabase.CreateAsset(textAsset, shaderPath);
				}
				else if (textAsset.text != sourceCode)
				{
					File.WriteAllText(shaderPath, sourceCode);
					AssetDatabase.ImportAsset(shaderPath);
				}
			}
			else
			{
				AssetDatabase.DeleteAsset(shaderPath);
			}
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