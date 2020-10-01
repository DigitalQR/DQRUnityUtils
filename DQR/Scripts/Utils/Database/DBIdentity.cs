using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using DQR.Debug;

namespace DQR.Database
{
	[System.Serializable]
	public class DBIdentity : ISerializationCallbackReceiver, System.IEquatable<DBIdentity>
	{
		[SerializeField]
		private byte[] m_RawGUID;

		[SerializeField]
		private DBIndex m_DatabaseIndex = DBIndex.Invalid;

		[SerializeField]
		private bool m_IsAbstract = false;

		private System.Guid m_Guid = System.Guid.NewGuid();

		/// <summary>
		/// The assigned index into the referenced table
		/// </summary>
		public DBIndex DatabaseIndex
		{
			get => m_DatabaseIndex;
#if UNITY_EDITOR
			set => m_DatabaseIndex = value;
#endif
		}

		/// <summary>
		/// The unique ID for this entry that will never change (Even if the index does)
		/// </summary>
		public System.Guid GUID
		{
			get => m_Guid;
#if UNITY_EDITOR
			set => m_Guid = value;
#endif
		}

		/// <summary>
		/// This identity is just on a placeholder entity and shouldn't appear in any tables
		/// </summary>
		public bool IsAbstract
		{
			get => m_IsAbstract;
#if UNITY_EDITOR
			set => m_IsAbstract = value;
#endif
		}

		public void OnBeforeSerialize()
		{
			m_RawGUID = m_Guid.ToByteArray();
		}

		public void OnAfterDeserialize()
		{
			if (m_DatabaseIndex == DBIndex.Invalid)
			{
				Assert.Format(!m_IsAbstract, "Asset has invalid DBIndex {0}", this);
			}

			if (m_RawGUID == null)
				m_Guid = System.Guid.NewGuid();
			else
				m_Guid = new System.Guid(m_RawGUID);
		}

		public bool Equals(DBIdentity other)
		{
			if (m_Guid == other.m_Guid)
			{
				Assert.Format(m_DatabaseIndex == other.m_DatabaseIndex, "Found DBIdentities with matching GUIDs ({0}) but not Indices {1} != {2} (Assuming equal)", m_Guid, m_DatabaseIndex, other.m_DatabaseIndex);
				return true;
			}

			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is DBIdentity)
				return Equals(obj as DBIdentity);

			return false;
		}

		public override int GetHashCode()
		{
			return m_Guid.GetHashCode();
		}

		public override string ToString()
		{
			return "(" + m_DatabaseIndex.ToString() + ")[" + m_Guid.ToString() + "]";
		}
	}
}