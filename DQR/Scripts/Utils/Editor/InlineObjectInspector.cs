#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace DQR
{
	public class InlineObjectInspectorAttribute : PropertyAttribute
	{
	}

	[CustomPropertyDrawer(typeof(InlineObjectInspectorAttribute))]
	public class InlineObjectInspectorDrawer : PropertyDrawer
	{
		// Draw the property inside the given rect
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var e = Editor.CreateEditor(property.objectReferenceValue);

			position.height = 16;
			EditorGUI.PropertyField(position, property);
			position.y += 20;

			if (e != null)
			{
				position.x += 20;
				position.width -= 40;
				var so = e.serializedObject;
				so.Update();

				var prop = so.GetIterator();
				prop.NextVisible(true);
				while (prop.NextVisible(true))
				{
					position.height = 16;
					EditorGUI.PropertyField(position, prop);
					position.y += 20;
				}
				if (GUI.changed)
					so.ApplyModifiedProperties();
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float height = base.GetPropertyHeight(property, label);
			var e = Editor.CreateEditor(property.objectReferenceValue);
			if (e != null)
			{
				var so = e.serializedObject;
				var prop = so.GetIterator();
				prop.NextVisible(true);
				while (prop.NextVisible(true)) height += 20;
			}
			return height;
		}
	}
}
#endif