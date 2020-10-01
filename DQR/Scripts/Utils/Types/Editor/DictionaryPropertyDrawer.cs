#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;
using System.Reflection;

namespace DQR.Types
{
	[CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
	public class DictionaryPropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (property.isExpanded)
			{
				float height = 0.0f;

				var elementsProp = property.FindPropertyRelative("m_Elements");
				for (int i = 0; i < elementsProp.arraySize; ++i)
				{
					var elemProp = elementsProp.GetArrayElementAtIndex(i);
					var keyProp = elemProp.FindPropertyRelative("Key");
					var valueProp = elemProp.FindPropertyRelative("Value");
					
					float maxHeight = Mathf.Max(EditorGUI.GetPropertyHeight(keyProp), EditorGUI.GetPropertyHeight(valueProp));
					height += maxHeight;
				}

				return height + EditorExt.GetRowHeight(3);
			}
			else
			{
				return EditorExt.GetRowHeight();
			}
		}
	
		public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
		{
			Rect position = EditorExt.GetInitialRectRow(area);
			property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
			
			if (property.isExpanded)
			{
				position = EditorExt.IncrementRectIndent(position);
				var elementsProp = property.FindPropertyRelative("m_Elements");

				// Draw temp KVP for + buttong
				{
					position = EditorExt.IncrementRectRow(position);

					Rect keyArea, buttonArea;
					EditorExt.SplitColumnsPixelsFromRight(position, 20.0f, out keyArea, out buttonArea);

					var tempProp = property.FindPropertyRelative("m_TemporaryKVP");
					var keyProp = tempProp.FindPropertyRelative("Key");
					var valueProp = tempProp.FindPropertyRelative("Value");

					// Draw KVP
					{
						float origLabelWidth = EditorGUIUtility.labelWidth;

						keyArea.height = EditorGUI.GetPropertyHeight(keyProp);

						EditorGUI.PropertyField(keyArea, keyProp, new GUIContent("New Key"), true);

						float maxHeight = keyArea.height;

						if (maxHeight > position.height)
						{
							float delta = maxHeight - position.height;
							position.y += delta;
						}
					}

					if (GUI.Button(buttonArea, "+"))
					{
						// Just change append temporary flag rather than mess around with serialzation stuff above
						var dirtyFlagProp = property.FindPropertyRelative("m_AppendTemporary");
						dirtyFlagProp.boolValue = true;

						//elementsProp.arraySize++;
						//var newProp = elementsProp.GetArrayElementAtIndex(elementsProp.arraySize - 1);
						//var newKeyProp = newProp.FindPropertyRelative("Key");
						//var newValueProp = newProp.FindPropertyRelative("Value");
						//newKeyProp.SetPropertyValue(null);
						//newValueProp.SetPropertyValue(null);
						//
						//keyProp.SetPropertyValue(null);
						//valueProp.SetPropertyValue(null);
					}
				}

				for (int i = 0; i < elementsProp.arraySize; ++i)
				{
					position = EditorExt.IncrementRectRow(position);
					
					Rect kvpArea, buttonArea;
					EditorExt.SplitColumnsPixelsFromRight(position, 20.0f, out kvpArea, out buttonArea);

					var elemProp = elementsProp.GetArrayElementAtIndex(i);
					var keyProp = elemProp.FindPropertyRelative("Key");
					var valueProp = elemProp.FindPropertyRelative("Value");
					
					// Draw KVP
					{
						Rect lhs, rhs;
						EditorExt.SplitColumnsPercentage(kvpArea, 0.3f, out lhs, out rhs);

						float origLabelWidth = EditorGUIUtility.labelWidth;
						
						lhs.height = EditorGUI.GetPropertyHeight(keyProp);
						rhs.height = EditorGUI.GetPropertyHeight(valueProp);

						EditorGUIUtility.labelWidth = 0.001f;
						EditorGUI.PropertyField(lhs, keyProp, new GUIContent("Key"), true);
						
						if (EditorGUI.GetPropertyHeight(valueProp) <= EditorExt.GetRowHeight())
						{
							EditorGUIUtility.labelWidth = 0.001f;
						}
						else
						{
							EditorGUIUtility.labelWidth = origLabelWidth * 0.6f;
						}
						EditorGUI.PropertyField(rhs, valueProp, new GUIContent("Value"), true);
						
						EditorGUIUtility.labelWidth = origLabelWidth;

						float maxHeight = Mathf.Max(lhs.height, rhs.height);

						if (maxHeight > position.height)
						{
							float delta = maxHeight - position.height;
							position.y += delta;
						}
					}
						
					if (GUI.Button(buttonArea, "-"))
					{
						elemProp.SetPropertyValue(null);
						elementsProp.DeleteArrayElementAtIndex(i);
						--i;
					}
				}
								
				position = EditorExt.IncrementRectRow(position);
				if (GUI.Button(position, "Clear All"))
				{
					elementsProp.ClearArray();
				}
			}
		}
	}
}
#endif