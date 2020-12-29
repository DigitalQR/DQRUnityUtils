using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Types
{
	public abstract class PropertyPrefBase
	{
		private string m_Key;

		public PropertyPrefBase(string key)
		{
			m_Key = key;
		}

		public string PropertyKey
		{
			get => m_Key;
		}
	}

	public abstract class PropertyPref<T> : PropertyPrefBase
	{
		private T m_Value;

		public PropertyPref(string key, T defaultValue) : base(key)
		{
			m_Value = defaultValue;
		}

		public T Set(T value)
		{
			if (!m_Value.Equals(value))
				OnValueChange(m_Value, value);

			m_Value = value;
			return m_Value;
		}

		public T Get()
		{
			return m_Value;
		}

		protected abstract void OnValueChange(T from, T to);
		
		public static implicit operator T(PropertyPref<T> prop)
		{
			return prop.Get();
		}

		public override bool Equals(object obj)
		{
			if (obj is PropertyPref<T>)
			{
				var prop = obj as PropertyPref<T>;
				return PropertyKey == prop.PropertyKey;
			}

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return PropertyKey.GetHashCode();
		}

		public static bool operator ==(PropertyPref<T> a, PropertyPref<T> b)
		{
			return a.m_Value.Equals(b.m_Value);
		}

		public static bool operator !=(PropertyPref<T> a, PropertyPref<T> b)
		{
			return !a.m_Value.Equals(b.m_Value);
		}

		public static bool operator ==(PropertyPref<T> a, T b)
		{
			return a.m_Value.Equals(b);
		}

		public static bool operator !=(PropertyPref<T> a, T b)
		{
			return !a.m_Value.Equals(b);
		}
	}

	public class PropertyPrefInt : PropertyPref<int>
	{
		public PropertyPrefInt(string key, int defaultValue) : base(key, defaultValue)
		{
			Set(PlayerPrefs.GetInt(PropertyKey, defaultValue));
		}

		protected override void OnValueChange(int from, int to)
		{
			PlayerPrefs.SetInt(PropertyKey, to);
		}
	}

	public class PropertyPrefFloat : PropertyPref<float>
	{
		public PropertyPrefFloat(string key, float defaultValue) : base(key, defaultValue)
		{
			Set(PlayerPrefs.GetFloat(PropertyKey, defaultValue));
		}

		protected override void OnValueChange(float from, float to)
		{
			PlayerPrefs.SetFloat(PropertyKey, to);
		}
	}
	
	public class PropertyPrefBool : PropertyPref<bool>
	{
		public PropertyPrefBool(string key, bool defaultValue) : base(key, defaultValue)
		{
			Set(PlayerPrefs.GetInt(PropertyKey, defaultValue ? 1 : 0) != 0);
		}

		protected override void OnValueChange(bool from, bool to)
		{
			PlayerPrefs.SetInt(PropertyKey, to ? 1 : 0);
		}
	}

	public class PropertyPrefString : PropertyPref<string>
	{
		public PropertyPrefString(string key, string defaultValue) : base(key, defaultValue)
		{
			Set(PlayerPrefs.GetString(PropertyKey, defaultValue));
		}

		protected override void OnValueChange(string from, string to)
		{
			PlayerPrefs.SetString(PropertyKey, to);
		}
	}

	public class PropertyPrefVector2 : PropertyPref<Vector2>
	{
		public PropertyPrefVector2(string key, Vector2 defaultValue) : base(key, defaultValue)
		{
			Set(new Vector2(
				PlayerPrefs.GetFloat(PropertyKey + ".x", defaultValue.x),
				PlayerPrefs.GetFloat(PropertyKey + ".y", defaultValue.y)
			));
		}

		protected override void OnValueChange(Vector2 from, Vector2 to)
		{
			PlayerPrefs.SetFloat(PropertyKey + ".x", to.x);
			PlayerPrefs.SetFloat(PropertyKey + ".y", to.y);
		}
	}

	public class PropertyPrefVector3 : PropertyPref<Vector3>
	{
		public PropertyPrefVector3(string key, Vector3 defaultValue) : base(key, defaultValue)
		{
			Set(new Vector3(
				PlayerPrefs.GetFloat(PropertyKey + ".x", defaultValue.x),
				PlayerPrefs.GetFloat(PropertyKey + ".y", defaultValue.y),
				PlayerPrefs.GetFloat(PropertyKey + ".z", defaultValue.z)
			));
		}

		protected override void OnValueChange(Vector3 from, Vector3 to)
		{
			PlayerPrefs.SetFloat(PropertyKey + ".x", to.x);
			PlayerPrefs.SetFloat(PropertyKey + ".y", to.y);
			PlayerPrefs.SetFloat(PropertyKey + ".z", to.z);
		}
	}

	public class PropertyPrefVector4 : PropertyPref<Vector4>
	{
		public PropertyPrefVector4(string key, Vector4 defaultValue) : base(key, defaultValue)
		{
			Set(new Vector4(
				PlayerPrefs.GetFloat(PropertyKey + ".x", defaultValue.x),
				PlayerPrefs.GetFloat(PropertyKey + ".y", defaultValue.y),
				PlayerPrefs.GetFloat(PropertyKey + ".z", defaultValue.z),
				PlayerPrefs.GetFloat(PropertyKey + ".w", defaultValue.w)
			));
		}

		protected override void OnValueChange(Vector4 from, Vector4 to)
		{
			PlayerPrefs.SetFloat(PropertyKey + ".x", to.x);
			PlayerPrefs.SetFloat(PropertyKey + ".y", to.y);
			PlayerPrefs.SetFloat(PropertyKey + ".z", to.z);
			PlayerPrefs.SetFloat(PropertyKey + ".w", to.w);
		}
	}

	public class PropertyPrefVector2Int : PropertyPref<Vector2Int>
	{
		public PropertyPrefVector2Int(string key, Vector2Int defaultValue) : base(key, defaultValue)
		{
			Set(new Vector2Int(
				PlayerPrefs.GetInt(PropertyKey + ".x", defaultValue.x),
				PlayerPrefs.GetInt(PropertyKey + ".y", defaultValue.y)
			));
		}

		protected override void OnValueChange(Vector2Int from, Vector2Int to)
		{
			PlayerPrefs.SetInt(PropertyKey + ".x", to.x);
			PlayerPrefs.SetInt(PropertyKey + ".y", to.y);
		}
	}

	public class PropertyPrefVector3Int : PropertyPref<Vector3Int>
	{
		public PropertyPrefVector3Int(string key, Vector3Int defaultValue) : base(key, defaultValue)
		{
			Set(new Vector3Int(
				PlayerPrefs.GetInt(PropertyKey + ".x", defaultValue.x),
				PlayerPrefs.GetInt(PropertyKey + ".y", defaultValue.y),
				PlayerPrefs.GetInt(PropertyKey + ".z", defaultValue.z)
			));
		}

		protected override void OnValueChange(Vector3Int from, Vector3Int to)
		{
			PlayerPrefs.SetInt(PropertyKey + ".x", to.x);
			PlayerPrefs.SetInt(PropertyKey + ".y", to.y);
			PlayerPrefs.SetInt(PropertyKey + ".z", to.z);
		}
	}
}
