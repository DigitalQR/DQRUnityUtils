#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

namespace DQR.Types
{
	[CustomPropertyDrawer(typeof(SubTypeOfAttribute))]
	public class SerializableTypeEditor : PropertyDrawer
	{
		private int m_Index = 0;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var attr = attribute as SubTypeOfAttribute;
			EditorGUI.LabelField(position, label);

			position.x += 120;
			position.width -= 120;

			SerializedProperty nameProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_TypeName");
			string value = nameProperty.stringValue;

			string[] choices = GetOptions();
			m_Index = IndexOf(value, choices);
			m_Index = EditorGUI.Popup(position, m_Index > 0 ? m_Index : 0, choices);

			nameProperty.stringValue = (m_Index == 0 ? "" : choices[m_Index]);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label);
		}

		private int IndexOf(string value, string[] values)
		{
			for (int i = 0; i < values.Length; ++i)
				if (value == values[i])
					return i;
			return 0;
		}

		private string[] GetOptions()
		{
			SubTypeOfAttribute attr = attribute as SubTypeOfAttribute;
			List<string> options = new List<string>(attr.GetAssignableTypes());
			options.Sort();
			options.Insert(0, "Null");
			return options.ToArray();
		}
	}
}
#endif