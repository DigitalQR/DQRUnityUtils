#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DQR.Types
{
	[CustomPropertyDrawer(typeof(WeightedItemCollection<>), true)]
	public class WeightedItemCollectionDrawer : PropertyDrawer
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
					var weightProp = elemProp.FindPropertyRelative("m_Weight");
					var itemProp = elemProp.FindPropertyRelative("m_Item");

					float maxHeight = Mathf.Max(EditorGUI.GetPropertyHeight(weightProp), EditorGUI.GetPropertyHeight(itemProp));
					height += maxHeight;
				}
			
				return height + EditorExt.GetRowHeight(1);
			}
			else
			{
				return EditorExt.GetRowHeight();
			}
		}

		public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
		{
			Rect position = EditorExt.GetInitialRectRow(area);
			property.isExpanded = true;

			{
				position = EditorExt.DecrementRectRow(position);
				var elementsProp = property.FindPropertyRelative("m_Elements");

				for (int i = 0; i < elementsProp.arraySize; ++i)
				{
					position = EditorExt.IncrementRectRow(position);

					Rect kvpArea, buttonArea;
					EditorExt.SplitColumnsPixelsFromRight(position, 20.0f, out kvpArea, out buttonArea);

					var elemProp = elementsProp.GetArrayElementAtIndex(i);
					var weightProp = elemProp.FindPropertyRelative("m_Weight");
					var itemProp = elemProp.FindPropertyRelative("m_Item");

					// Draw weight:item as if KVP
					{
						Rect lhs, rhs;
						EditorExt.SplitColumnsPercentage(kvpArea, 0.7f, out lhs, out rhs);

						float origLabelWidth = EditorGUIUtility.labelWidth;

						lhs.height = EditorGUI.GetPropertyHeight(itemProp);
						rhs.height = EditorGUI.GetPropertyHeight(weightProp);

						EditorGUIUtility.labelWidth = 0.001f;
						EditorGUI.PropertyField(rhs, weightProp, new GUIContent("Weight"), true);

						if (EditorGUI.GetPropertyHeight(itemProp) <= EditorExt.GetRowHeight())
						{
							EditorGUIUtility.labelWidth = 0.001f;
						}
						else
						{
							EditorGUIUtility.labelWidth = origLabelWidth * 0.6f;
						}
						EditorGUI.PropertyField(lhs, itemProp, new GUIContent("Item"), true);

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

				Rect newArea, clearArea;
				EditorExt.SplitColumnsPercentage(position, 0.65f, out newArea, out clearArea);
				
				if (GUI.Button(newArea, "New Item"))
				{
					elementsProp.arraySize++;
					var newProp = elementsProp.GetArrayElementAtIndex(elementsProp.arraySize - 1);
					var newWeightProp = newProp.FindPropertyRelative("m_Weight");
					var newItemProp = newProp.FindPropertyRelative("m_Item");
					newWeightProp.SetPropertyValue(null);
					newItemProp.SetPropertyValue(null);
				}
				
				if (GUI.Button(clearArea, "Clear All Items"))
				{
					elementsProp.ClearArray();
				}
			}
		}
	}

	[CustomPropertyDrawer(typeof(WeightedGroupCollection<>), true)]
	public class WeightedGroupCollectionDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (property.isExpanded)
			{
				var primaryGroupProp = property.FindPropertyRelative("m_PrimaryGroup");
				float height = EditorExt.GetRowHeight(1);

				if (primaryGroupProp.isExpanded)
				{
					height += EditorGUI.GetPropertyHeight(primaryGroupProp);
				}
			
				var elementsProp = property.FindPropertyRelative("m_Subgroups");
				for (int i = 0; i < elementsProp.arraySize; ++i)
				{
					height += EditorExt.GetRowHeight(1);

					var elemProp = elementsProp.GetArrayElementAtIndex(i);
					if (elemProp.isExpanded)
					{
						var collectionProp = elemProp.FindPropertyRelative("m_Collection");
						height += EditorGUI.GetPropertyHeight(collectionProp);
					}
				}
			
				return height + EditorExt.GetRowHeight(2);
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
				position = EditorExt.IncrementRectRow(position);

				var primaryGroupProp = property.FindPropertyRelative("m_PrimaryGroup");
				var primaryGroupPropElementsProp = primaryGroupProp.FindPropertyRelative("m_Elements");
				primaryGroupProp.isExpanded = EditorGUI.Foldout(position, primaryGroupProp.isExpanded, new GUIContent("Root (" + primaryGroupPropElementsProp.arraySize + ")"));

				if(primaryGroupProp.isExpanded)
				{
					position = EditorExt.IncrementRectIndent(position);
					position = EditorExt.IncrementRectRow(position);

					Rect primaryGroupArea = position;
					float primaryGroupHeight = EditorGUI.GetPropertyHeight(primaryGroupProp);
					primaryGroupArea.height = primaryGroupHeight;
					EditorGUI.PropertyField(primaryGroupArea, primaryGroupProp);

					if (primaryGroupHeight > position.height)
					{
						float delta = primaryGroupHeight - position.height;
						position.y += delta;
					}

					position = EditorExt.DecrementRectIndent(position);
				}
				
				var subgroupsProp = property.FindPropertyRelative("m_Subgroups");
				float origLabelWidth = EditorGUIUtility.labelWidth;

				for (int i = 0; i < subgroupsProp.arraySize; ++i)
				{
					position = EditorExt.IncrementRectRow(position);

					Rect kvpArea, buttonArea;
					EditorExt.SplitColumnsPixelsFromRight(position, 20.0f, out kvpArea, out buttonArea);

					Rect lhsArea, rhsArea;
					EditorExt.SplitColumnsPercentage(kvpArea, 0.5f, out lhsArea, out rhsArea);

					var groupProp = subgroupsProp.GetArrayElementAtIndex(i);
					var weightProp = groupProp.FindPropertyRelative("m_Weight");
					var collectionProp = groupProp.FindPropertyRelative("m_Collection");

					var collectionElementsProp = collectionProp.FindPropertyRelative("m_Elements");
					groupProp.isExpanded = EditorGUI.Foldout(lhsArea, groupProp.isExpanded, new GUIContent("Subgroup " + i + " (" + collectionElementsProp.arraySize + ")"));
					EditorGUIUtility.labelWidth = 0.001f;
					EditorGUI.PropertyField(rhsArea, weightProp);
					EditorGUIUtility.labelWidth = origLabelWidth;
					
					if (groupProp.isExpanded)
					{
						position = EditorExt.IncrementRectIndent(position);
						position = EditorExt.IncrementRectRow(position);

						Rect collectionArea = position;
						float collectionHeight = EditorGUI.GetPropertyHeight(collectionProp);
						collectionArea.height = collectionHeight;
						EditorGUI.PropertyField(collectionArea, collectionProp);
						
						if (collectionHeight > position.height)
						{
							float delta = collectionHeight - position.height;
							position.y += delta;
						}

						position = EditorExt.DecrementRectIndent(position);
					}
					
					if (GUI.Button(buttonArea, "-"))
					{
						groupProp.SetPropertyValue(null);
						subgroupsProp.DeleteArrayElementAtIndex(i);
						--i;
					}
				}

				position = EditorExt.IncrementRectRow(position);

				Rect newArea, clearArea;
				EditorExt.SplitColumnsPercentage(position, 0.65f, out newArea, out clearArea);
				
				if (GUI.Button(newArea, "New Group"))
				{
					subgroupsProp.arraySize++;
					var newProp = subgroupsProp.GetArrayElementAtIndex(subgroupsProp.arraySize - 1);
					var newWeightProp = newProp.FindPropertyRelative("m_Weight");
					var newCollectionProp = newProp.FindPropertyRelative("m_Collection");
					var newElementsProp = newCollectionProp.FindPropertyRelative("m_Elements");
					newWeightProp.SetPropertyValue(null);
					newCollectionProp.SetPropertyValue(null);
					newElementsProp.arraySize = 0;
				}
				
				if (GUI.Button(clearArea, "Clear All Group"))
				{
					subgroupsProp.ClearArray();
				}

			}
		}
	}
}
#endif