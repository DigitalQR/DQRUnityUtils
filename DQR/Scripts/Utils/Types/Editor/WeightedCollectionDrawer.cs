#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DQR.Types
{
	[CustomPropertyDrawer(typeof(WeightedCollection<>), true)]
	public class WeightedCollectionDrawer : PropertyDrawer
	{
		private float GetElemHeight(SerializedProperty collectionList, int index)
		{
			var currProp = collectionList.GetArrayElementAtIndex(index);

			if (currProp.isExpanded)
			{
				float height = EditorExt.GetRowHeight(1);
				
				// Items height
				{
					height += EditorExt.GetRowHeight(1);
					var itemsProp = currProp.FindPropertyRelative("Items");

					for (int i = 0; i < itemsProp.arraySize; ++i)
					{
						var itemElemProp = itemsProp.GetArrayElementAtIndex(i);
						var weightProp = itemElemProp.FindPropertyRelative("Weight");
						var itemProp = itemElemProp.FindPropertyRelative("Item");

						height += EditorExt.KeyValuePairHeight(weightProp, itemProp);
					}
				}

				// Sub groups height
				{
					var subIndicesProp = currProp.FindPropertyRelative("SubcollectionIndices");
					for (int i = 0; i < subIndicesProp.arraySize; ++i)
					{
						int subIndex = subIndicesProp.GetArrayElementAtIndex(i).intValue;
						height += GetElemHeight(collectionList, subIndex);
					}
				}

				return height;
			}
			else
			{
				return EditorExt.GetRowHeight(1);
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var collectionList = property.FindPropertyRelative("m_SerializedCollections");

			if (collectionList.arraySize != 0)
				return GetElemHeight(collectionList, 0);
			else
				return EditorExt.GetRowHeight(1);
		}

		public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
		{
			Rect position = EditorExt.GetInitialRectRow(area);

			var collectionList = property.FindPropertyRelative("m_SerializedCollections");

			// Make sure root always exists
			if (collectionList.arraySize == 0)
			{
				collectionList.arraySize = 1;

				var newGroupProp = collectionList.GetArrayElementAtIndex(0);
				newGroupProp.FindPropertyRelative("Weight").SetPropertyValue(1.0f);
				newGroupProp.FindPropertyRelative("Items").arraySize = 0;
				newGroupProp.FindPropertyRelative("SubcollectionIndices").arraySize = 0;
			}

			// Insert root, if it doesn't exist			
			DrawCollection(ref position, collectionList, 0, label);
		}

		protected void GatherCollectionsToDelete(List<int> indices, SerializedProperty collectionList, int index)
		{
			indices.Add(index);
			SerializedProperty currProp = collectionList.GetArrayElementAtIndex(index);

			var subIndicesProp = currProp.FindPropertyRelative("SubcollectionIndices");
			for (int i = 0; i < subIndicesProp.arraySize; ++i)
			{
				int subIndex = subIndicesProp.GetArrayElementAtIndex(i).intValue;
				GatherCollectionsToDelete(indices, collectionList, subIndex);
			}
		}

		protected void DeleteGroupAtIndex(SerializedProperty collectionList, int index)
		{
			var deleteProp = collectionList.GetArrayElementAtIndex(index);
			
			// Delete sub groups first
			{
				var subIndicesProp = deleteProp.FindPropertyRelative("SubcollectionIndices");
				for (int i = 0; i < subIndicesProp.arraySize; ++i)
				{
					int subIndex = subIndicesProp.GetArrayElementAtIndex(i).intValue;
					DeleteGroupAtIndex(collectionList, subIndex);
				}
			}

			// Correct indices of any remaining groups
			for (int c = 0; c < collectionList.arraySize; ++c)
			{
				var currProp = collectionList.GetArrayElementAtIndex(c);
				var subIndicesProp = currProp.FindPropertyRelative("SubcollectionIndices");

				for (int i = 0; i < subIndicesProp.arraySize; ++i)
				{
					var subGroupIndex = subIndicesProp.GetArrayElementAtIndex(i);
					int subIndex = subGroupIndex.intValue;

					if (subIndex > index)
						subGroupIndex.intValue--;
					else if (subIndex == index)
					{
						subIndicesProp.DeleteArrayElementAtIndex(i);
						i--;
					}
				}
			}

			collectionList.DeleteArrayElementAtIndex(index);
		}

		protected void DrawCollection(ref Rect position, SerializedProperty collectionList, int index, GUIContent label)
		{
			SerializedProperty currProp = collectionList.GetArrayElementAtIndex(index);
			var subIndicesProp = currProp.FindPropertyRelative("SubcollectionIndices");
			var weightProp = currProp.FindPropertyRelative("Weight");
			var itemsProp = currProp.FindPropertyRelative("Items");

			Rect foldoutRect = position;

			if (index != 0)
			{
				EditorExt.SplitColumnsPixelsFromRight(position, 20.0f, out Rect dropdown, out Rect buttonArea);
				EditorExt.SplitColumnsPercentage(dropdown, 0.5f, out Rect lhs, out Rect rhs);
				foldoutRect = lhs;
				
				EditorGUI.PropertyField(rhs, weightProp, GUIContent.none, true);

				if (GUI.Button(buttonArea, "-"))
				{
					DeleteGroupAtIndex(collectionList, index);
					return;
				}
			}

			currProp.isExpanded = EditorGUI.Foldout(foldoutRect, currProp.isExpanded, label);

			if (currProp.isExpanded)
			{
				position = EditorExt.IncrementRectIndent(position);
				position = EditorExt.IncrementRectRow(position);
				
				// + buttons
				{
					EditorExt.SplitColumnsPercentage(position, 0.5f, out Rect lhs, out Rect rhs);

					if (GUI.Button(lhs, "Add Item(" + itemsProp.arraySize + ")"))
					{
						itemsProp.arraySize++;
						var newElemProp = itemsProp.GetArrayElementAtIndex(itemsProp.arraySize - 1);
						newElemProp.FindPropertyRelative("Weight").SetPropertyValue(1.0f);
						newElemProp.FindPropertyRelative("Item").SetPropertyValue(null);
					}

					if (GUI.Button(rhs, "Add Group(" + subIndicesProp.arraySize + ")"))
					{
						// Insert an end then swap
						collectionList.arraySize++;
						int collectionIndex = collectionList.arraySize - 1;
						var newGroupProp = collectionList.GetArrayElementAtIndex(collectionIndex);

						newGroupProp.FindPropertyRelative("Weight").SetPropertyValue(1.0f);
						newGroupProp.FindPropertyRelative("Items").arraySize = 0;
						newGroupProp.FindPropertyRelative("SubcollectionIndices").arraySize = 0;
						
						subIndicesProp.arraySize++;
						subIndicesProp.GetArrayElementAtIndex(subIndicesProp.arraySize - 1).intValue = collectionIndex;
					}
				}

				// Draw items
				for (int i = 0; i < itemsProp.arraySize; ++i)
				{
					position = EditorExt.IncrementRectRow(position);

					var elemProp = itemsProp.GetArrayElementAtIndex(i);
					var elemWeightProp = elemProp.FindPropertyRelative("Weight");
					var elemItemProp = elemProp.FindPropertyRelative("Item");

					if (EditorExt.KeyValuePairField(ref position, elemWeightProp, elemItemProp, "-"))
					{
						elemProp.SetPropertyValue(null);
						itemsProp.DeleteArrayElementAtIndex(i);
						--i;
					}
				}

				// Draw subgroups
				for (int i = 0; i < subIndicesProp.arraySize; ++i)
				{
					int subIndex = subIndicesProp.GetArrayElementAtIndex(i).intValue;
					
					position = EditorExt.IncrementRectRow(position);
					index++;
					DrawCollection(ref position, collectionList, subIndex, new GUIContent("Subgroup(" + i  + ")"));
				}

				position = EditorExt.DecrementRectIndent(position);
			}
		}
	}
}
#endif
