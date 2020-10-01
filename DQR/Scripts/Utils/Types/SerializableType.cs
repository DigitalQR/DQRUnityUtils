using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using DQR.Debug;

namespace DQR.Types
{
	[System.Serializable]
	public class SerializableType : ISerializationCallbackReceiver
	{
		[SerializeField]
		private string m_TypeName;

		private System.Type m_Type;

		public SerializableType(System.Type type = null)
		{
			m_Type = type;
			m_TypeName = m_Type != null ? m_Type.Name : "";
		}

		public System.Type AssignedType
		{
			get => m_Type;
		}

		public string AssignedTypeName
		{
			get => m_TypeName;
		}

		private void UpdateAssignedType()
		{
			if (string.IsNullOrEmpty(m_TypeName))
			{
				m_Type = null;
			}
			else
			{

				foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
				{
					System.Type type = assembly.GetType(m_TypeName);
					if (type != null)
					{
						m_Type = type;
						break;
					}
				}

				if (m_Type == null)
				{
					Assert.FailFormat("Unable to find System.Type '{0}'", m_TypeName);
					m_TypeName = "";
				}
			}
		}

		public void OnBeforeSerialize()
		{
			UpdateAssignedType();
		}

		public void OnAfterDeserialize()
		{
			UpdateAssignedType();
		}

		public bool IsAssignableFrom(System.Type other)
		{
			return m_Type != null ? m_Type.IsAssignableFrom(other) : false;
		}

		public bool IsAssignableTo(System.Type other)
		{
			return m_Type != null ? other.IsAssignableFrom(m_Type) : false;
		}

		public bool IsSubclassOf(System.Type other)
		{
			return m_Type != null ? m_Type.IsSubclassOf(other) : false;
		}

		public bool IsParentClassOf(System.Type other)
		{
			return m_Type != null ? other.IsSubclassOf(m_Type) : false;
		}
	}
}
