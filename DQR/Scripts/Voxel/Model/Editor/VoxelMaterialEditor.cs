#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;
using System.Reflection;

namespace DQR.Voxel.Model
{

	[CustomEditor(typeof(VoxelMaterial))]
	//[CanEditMultipleObjects]
	public class VoxelMaterialEditor : Editor
	{
		private VoxelMaterialLayout m_Layout = null;

		private SerializedProperty m_Prop_Layout;
		private SerializedProperty m_Prop_RenderMaterial;
		private SerializedProperty m_Prop_MaterialData = null;
		private SerializedProperty[] m_Prop_MaterialDataArray = null;

		void OnEnable()
		{
			m_Prop_Layout = serializedObject.FindProperty("m_Layout");
			m_Prop_RenderMaterial = serializedObject.FindProperty("m_RenderMaterial");
			m_Prop_MaterialData = serializedObject.FindProperty("m_MaterialData");

			m_Layout = m_Prop_Layout.objectReferenceValue as VoxelMaterialLayout;

			m_Prop_MaterialDataArray = new SerializedProperty[m_Prop_MaterialData.arraySize];
			for (int i = 0; i < m_Prop_MaterialDataArray.Length; ++i)
				m_Prop_MaterialDataArray[i] = m_Prop_MaterialData.GetArrayElementAtIndex(i);
		}

		public override void OnInspectorGUI()
		{
			bool changedDetected = false;
			changedDetected |= EditorGUILayout.PropertyField(m_Prop_RenderMaterial);
			changedDetected |= EditorGUILayout.PropertyField(m_Prop_Layout);

			if (m_Layout != null)
			{
				float[] startData = m_Prop_MaterialDataArray.Select((p) => p.floatValue).ToArray();

				int offset = 0;
				for (int i = 0; i < m_Layout.LayoutCount; ++i)
				{
					var desc = m_Layout.GetLayoutDesc(i);

					switch (desc.DataFormat)
					{
						case VoxelMaterialLayoutDesc.DataLayout.Float1:
							if(desc.ShouldClampRange)
								m_Prop_MaterialDataArray[offset].floatValue = EditorGUILayout.Slider(desc.Identifier, m_Prop_MaterialDataArray[offset].floatValue, desc.MinRange, desc.MaxRange);
							else
								m_Prop_MaterialDataArray[offset].floatValue = EditorGUILayout.FloatField(desc.Identifier, m_Prop_MaterialDataArray[offset].floatValue);
							break;

						case VoxelMaterialLayoutDesc.DataLayout.Float2:
							{
								Vector2 val = new Vector2(m_Prop_MaterialDataArray[offset + 0].floatValue, m_Prop_MaterialDataArray[offset + 1].floatValue);
								val = EditorGUILayout.Vector2Field(desc.Identifier, val);

								m_Prop_MaterialDataArray[offset + 0].floatValue = val.x;
								m_Prop_MaterialDataArray[offset + 1].floatValue = val.y;
								break;
							}

						case VoxelMaterialLayoutDesc.DataLayout.Float3:
							{
								Vector3 val = new Vector3(m_Prop_MaterialDataArray[offset + 0].floatValue, m_Prop_MaterialDataArray[offset + 1].floatValue, m_Prop_MaterialDataArray[offset + 2].floatValue);
								val = EditorGUILayout.Vector3Field(desc.Identifier, val);

								m_Prop_MaterialDataArray[offset + 0].floatValue = val.x;
								m_Prop_MaterialDataArray[offset + 1].floatValue = val.y;
								m_Prop_MaterialDataArray[offset + 2].floatValue = val.z;
								break;
							}

						case VoxelMaterialLayoutDesc.DataLayout.Float4:
							{
								Vector4 val = new Vector4(m_Prop_MaterialDataArray[offset + 0].floatValue, m_Prop_MaterialDataArray[offset + 1].floatValue, m_Prop_MaterialDataArray[offset + 2].floatValue, m_Prop_MaterialDataArray[offset + 3].floatValue);
								val = EditorGUILayout.Vector4Field(desc.Identifier, val);

								m_Prop_MaterialDataArray[offset + 0].floatValue = val.x;
								m_Prop_MaterialDataArray[offset + 1].floatValue = val.y;
								m_Prop_MaterialDataArray[offset + 2].floatValue = val.z;
								m_Prop_MaterialDataArray[offset + 3].floatValue = val.w;
								break;
							}

						case VoxelMaterialLayoutDesc.DataLayout.Colour:
							{
								Vector4 val = new Vector4(m_Prop_MaterialDataArray[offset + 0].floatValue, m_Prop_MaterialDataArray[offset + 1].floatValue, m_Prop_MaterialDataArray[offset + 2].floatValue, m_Prop_MaterialDataArray[offset + 3].floatValue);
								val = EditorGUILayout.ColorField(desc.Identifier, val);

								m_Prop_MaterialDataArray[offset + 0].floatValue = val.x;
								m_Prop_MaterialDataArray[offset + 1].floatValue = val.y;
								m_Prop_MaterialDataArray[offset + 2].floatValue = val.z;
								m_Prop_MaterialDataArray[offset + 3].floatValue = val.w;
								break;
							}
					}

					offset += desc.DataFormat.GetChannelCount();
				}
				
				for (int i = 0; i < startData.Length; ++i)
				{
					if (m_Prop_MaterialDataArray[i].floatValue != startData[i])
					{
						changedDetected = true;
						break;
					}
				}
			}

			EditorGUILayout.Space();

			bool before = GUI.enabled;
			GUI.enabled = false;
			m_Prop_MaterialData.isExpanded = true;
			EditorGUILayout.PropertyField(m_Prop_MaterialData, true);
			GUI.enabled = before;

			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
