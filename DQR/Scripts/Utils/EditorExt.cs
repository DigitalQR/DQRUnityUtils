#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DQR
{

	public static class EditorExt 
	{
		public static float GetRowHeight(int count = 1)
		{
			return EditorGUIUtility.singleLineHeight * count;
		}

		public static Rect GetInitialRectRow(Rect area)
		{
			float height = GetRowHeight();

			Rect newPosition = new Rect();
			newPosition.xMin = area.xMin;
			newPosition.xMax = area.xMax;
			newPosition.yMin = area.yMin;
			newPosition.yMax = area.yMin + height;
			return newPosition;
		}

		public static Rect IncrementRectRow(Rect position)
		{
			float height = GetRowHeight(1);

			Rect newPosition = new Rect();
			newPosition.xMin = position.xMin;
			newPosition.xMax = position.xMax;
			newPosition.yMin = position.yMin + height;
			newPosition.yMax = position.yMax + height;
			return newPosition;
		}

		public static Rect DecrementRectRow(Rect position)
		{
			float height = GetRowHeight(1);

			Rect newPosition = new Rect();
			newPosition.xMin = position.xMin;
			newPosition.xMax = position.xMax;
			newPosition.yMin = position.yMin - height;
			newPosition.yMax = position.yMax - height;
			return newPosition;
		}

		public static Rect IncrementRectIndent(Rect position, int count = 1)
		{
			float height = GetRowHeight();
			EditorGUI.indentLevel += count;

			Rect newPosition = new Rect();
			newPosition.xMin = position.xMin + EditorGUIUtility.singleLineHeight * count;
			newPosition.xMax = position.xMax;
			newPosition.yMin = position.yMin;
			newPosition.yMax = position.yMin + height;
			return newPosition;
		}

		public static Rect DecrementRectIndent(Rect position, int count = 1)
		{
			float height = GetRowHeight();
			EditorGUI.indentLevel -= count;

			Rect newPosition = new Rect();
			newPosition.xMin = position.xMin - EditorGUIUtility.singleLineHeight * count;
			newPosition.xMax = position.xMax;
			newPosition.yMin = position.yMin;
			newPosition.yMax = position.yMin + height;
			return newPosition;
		}

		public static void SplitColumnsPercentage(Rect area, float t, out Rect lhs, out Rect rhs)
		{
			lhs = rhs = area;
			lhs.xMax = lhs.xMin + lhs.width * t;
			rhs.xMin = lhs.xMax;
		}

		public static void SplitColumnsPixelsFromLeft(Rect area, float count, out Rect lhs, out Rect rhs)
		{
			lhs = rhs = area;
			lhs.xMax = lhs.xMin + count;
			rhs.xMin = lhs.xMax;
		}

		public static void SplitColumnsPixelsFromRight(Rect area, float count, out Rect lhs, out Rect rhs)
		{
			lhs = rhs = area;
			rhs.xMin = rhs.xMax - count;
			lhs.xMax = rhs.xMin;
		}


		public static void SetPropertyDefault(SerializedProperty prop)
		{
			if (prop == null) throw new System.ArgumentNullException("prop");

			switch (prop.propertyType)
			{
				case SerializedPropertyType.Integer:
					prop.intValue = 0;
					break;
				case SerializedPropertyType.Boolean:
					prop.boolValue = false;
					break;
				case SerializedPropertyType.Float:
					prop.floatValue = 0f;
					break;
				case SerializedPropertyType.String:
					prop.stringValue = string.Empty;
					break;
				case SerializedPropertyType.Color:
					prop.colorValue = Color.black;
					break;
				case SerializedPropertyType.ObjectReference:
					prop.objectReferenceValue = null;
					break;
				case SerializedPropertyType.LayerMask:
					prop.intValue = -1;
					break;
				case SerializedPropertyType.Enum:
					prop.enumValueIndex = 0;
					break;
				case SerializedPropertyType.Vector2:
					prop.vector2Value = Vector2.zero;
					break;
				case SerializedPropertyType.Vector3:
					prop.vector3Value = Vector3.zero;
					break;
				case SerializedPropertyType.Vector4:
					prop.vector4Value = Vector4.zero;
					break;
				case SerializedPropertyType.Rect:
					prop.rectValue = Rect.zero;
					break;
				case SerializedPropertyType.ArraySize:
					prop.arraySize = 0;
					break;
				case SerializedPropertyType.Character:
					prop.intValue = 0;
					break;
				case SerializedPropertyType.AnimationCurve:
					prop.animationCurveValue = null;
					break;
				case SerializedPropertyType.Bounds:
					prop.boundsValue = default(Bounds);
					break;
				case SerializedPropertyType.Gradient:
					throw new System.InvalidOperationException("Can not handle Gradient types.");
			}
		}

		public static void RemoveElement(this SerializedProperty list, int index)
		{
			if (list == null)
				throw new ArgumentNullException();

			if (!list.isArray)
				throw new ArgumentException("Property is not an array");

			if (index < 0 || index >= list.arraySize)
				throw new IndexOutOfRangeException();

			list.GetArrayElementAtIndex(index).SetPropertyValue(null);
			list.DeleteArrayElementAtIndex(index);

			list.serializedObject.ApplyModifiedProperties();
		}

		public static bool KeyValuePairField(ref Rect position, SerializedProperty keyProp, SerializedProperty valueProp, string buttonText = null)
		{
			Rect kvpArea, buttonArea;
			if (buttonText != null)
			{
				EditorExt.SplitColumnsPixelsFromRight(position, 20.0f, out kvpArea, out buttonArea);
			}
			else
			{
				kvpArea = position;
				buttonArea = new Rect();
			}
			
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

			if (buttonText != null && GUI.Button(buttonArea, buttonText))
			{
				return true;
			}

			return false;
		}

		public static float KeyValuePairHeight(SerializedProperty keyProp, SerializedProperty valueProp, int rowHeight = 1)
		{
			float keyHeight = keyProp.isExpanded ? EditorExt.GetRowHeight(rowHeight) : EditorGUI.GetPropertyHeight(keyProp);
			float valueHeight = valueProp.isExpanded ? EditorExt.GetRowHeight(rowHeight) : EditorGUI.GetPropertyHeight(valueProp);

			return Mathf.Max(keyHeight, valueHeight);
		}

		public static void SetPropertyValue(this SerializedProperty property, object value)
		{
			switch (property.propertyType)
			{

				case SerializedPropertyType.AnimationCurve:
					property.animationCurveValue = value as AnimationCurve;
					break;

				case SerializedPropertyType.ArraySize:
					property.intValue = Convert.ToInt32(value);
					break;

				case SerializedPropertyType.Boolean:
					property.boolValue = Convert.ToBoolean(value);
					break;

				case SerializedPropertyType.Bounds:
					property.boundsValue = (value == null)
							? new Bounds()
							: (Bounds)value;
					break;

				case SerializedPropertyType.Character:
					property.intValue = Convert.ToInt32(value);
					break;

				case SerializedPropertyType.Color:
					property.colorValue = (value == null)
							? new Color()
							: (Color)value;
					break;

				case SerializedPropertyType.Float:
					property.floatValue = Convert.ToSingle(value);
					break;

				case SerializedPropertyType.Integer:
					property.intValue = Convert.ToInt32(value);
					break;

				case SerializedPropertyType.LayerMask:
					property.intValue = (value is LayerMask) ? ((LayerMask)value).value : Convert.ToInt32(value);
					break;

				case SerializedPropertyType.ObjectReference:
					property.objectReferenceValue = value as UnityEngine.Object;
					break;

				case SerializedPropertyType.Quaternion:
					property.quaternionValue = (value == null)
							? Quaternion.identity
							: (Quaternion)value;
					break;

				case SerializedPropertyType.Rect:
					property.rectValue = (value == null)
							? new Rect()
							: (Rect)value;
					break;

				case SerializedPropertyType.String:
					property.stringValue = value as string;
					break;

				case SerializedPropertyType.Vector2:
					property.vector2Value = (value == null)
							? Vector2.zero
							: (Vector2)value;
					break;

				case SerializedPropertyType.Vector3:
					property.vector3Value = (value == null)
							? Vector3.zero
							: (Vector3)value;
					break;

				case SerializedPropertyType.Vector4:
					property.vector4Value = (value == null)
							? Vector4.zero
							: (Vector4)value;
					break;

			}
		}

		public static object GetTargetObjectOfProperty(SerializedProperty prop)
		{
			if (prop == null) return null;

			var path = prop.propertyPath.Replace(".Array.data[", "[");
			object obj = prop.serializedObject.targetObject;
			var elements = path.Split('.');
			foreach (var element in elements)
			{
				if (element.Contains("["))
				{
					var elementName = element.Substring(0, element.IndexOf("["));
					var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
					obj = GetValue_Imp(obj, elementName, index);
				}
				else
				{
					obj = GetValue_Imp(obj, element);
				}
			}
			return obj;
		}

		private static object GetValue_Imp(object source, string name)
		{
			if (source == null)
				return null;
			var type = source.GetType();

			while (type != null)
			{
				var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				if (f != null)
					return f.GetValue(source);

				var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if (p != null)
					return p.GetValue(source, null);

				type = type.BaseType;
			}
			return null;
		}

		private static object GetValue_Imp(object source, string name, int index)
		{
			var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
			if (enumerable == null) return null;
			var enm = enumerable.GetEnumerator();
			//while (index-- >= 0)
			//    enm.MoveNext();
			//return enm.Current;

			for (int i = 0; i <= index; i++)
			{
				if (!enm.MoveNext()) return null;
			}
			return enm.Current;
		}
	}
}
#endif