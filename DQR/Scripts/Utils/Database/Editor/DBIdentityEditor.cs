#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DQR.Database
{
	[CustomPropertyDrawer(typeof(DBIdentity))]
	public class DBIdentityEditor : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			DBIdentity id = fieldInfo.GetValue(property.serializedObject.targetObject) as DBIdentity;

			EditorGUI.BeginProperty(position, label, property);

			Color startColour = GUI.color;
			GUI.color = Color.black;
			EditorGUI.LabelField(position, property.displayName);

			Rect buttonRect = new Rect();
			buttonRect.x = position.width - 120;
			buttonRect.y = position.y;
			buttonRect.width = 120;
			buttonRect.height = 17;
			if (id.IsAbstract)
				GUI.color = Color.magenta;
			else
				GUI.color = Color.white;

			if (GUI.Button(buttonRect, "Toggle Abstract"))
			{
				id.IsAbstract = !id.IsAbstract;
				id.DatabaseIndex = DBIndex.Invalid;
				EditorUtility.SetDirty(property.serializedObject.targetObject);
			}

			position.x += 15;
			position.y += 35;

			if (id.IsAbstract)
			{
				GUI.color = Color.magenta;
				EditorGUI.LabelField(position, "This is marked as abstract so will not appear in any table", EditorStyles.whiteMiniLabel);
			}
			else if (id.DatabaseIndex == DBIndex.Invalid)
			{
				GUI.color = Color.red;
				EditorGUI.LabelField(position, "Index: INVALID", EditorStyles.whiteLabel);
			}
			else
			{
				GUI.color = Color.grey;
				EditorGUI.LabelField(position, "Index: " + id.DatabaseIndex, EditorStyles.whiteLabel);
			}

			position.y += 15;
			{
				GUI.color = Color.grey;
				EditorGUI.LabelField(position, "GUID: " + id.GUID, EditorStyles.whiteLabel);
			}

			EditorGUI.EndProperty();
			GUI.color = startColour;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label) + 40;
		}
	}
}

#endif