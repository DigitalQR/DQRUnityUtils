using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DQR.Database
{
	[System.Serializable]
	public class IndexedBehaviour : MonoBehaviour, IDatabaseIndexable, ISerializationCallbackReceiver
	{
		[SerializeField]
		private DBIdentity m_DatabaseIdentity = null;

		public DBIdentity DatabaseIdentity
		{
			get => m_DatabaseIdentity;
		}

#if UNITY_EDITOR
		[SerializeField, HideInInspector]
		private string m_AttachedInstanceGUID;
#endif

		public void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			// Track metadata GUID and reset if missmatch (This handle duplicates)
			if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(GetInstanceID(), out string guid, out long localID))
			{
				if (m_AttachedInstanceGUID != guid)
				{
					Assert.FailFormat("Detected invalid DBIdentity resetting for {0}", this);
					m_DatabaseIdentity = new DBIdentity();
					m_AttachedInstanceGUID = guid;
					EditorUtility.SetDirty(this);
				}
			}
#endif
		}

		public void OnAfterDeserialize()
		{
		}
	}
}
