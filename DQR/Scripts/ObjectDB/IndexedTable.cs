using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
#endif

namespace DQR.Database
{
	[System.Serializable]
	public abstract class IndexedTable<IndexedObjectType> : ScriptableObject, ISerializationCallbackReceiver where IndexedObjectType : Object, IDatabaseIndexable
	{
		private IndexedObjectType[] m_ObjectTable = null;

		[SerializeField]
		private Object[] m_SerializedTable = null;

#if UNITY_EDITOR
		[SerializeField]
		private string m_SearchDirectory = "Assets/Prefabs";
#endif

		public int Count
		{
			get => m_ObjectTable != null ? m_ObjectTable.Length : 0;
		}

		public IEnumerable<IndexedObjectType> AllElements
		{
			get => m_ObjectTable;
		}

		public IndexedObjectType GetAt(DBIndex index)
		{
			if (m_ObjectTable != null)
			{
				if (index.Offset >= Count)
					return default;

				return m_ObjectTable[index.Offset];
			}

			return default;
		}

#if UNITY_EDITOR
		public void RefreshTable()
		{
			Dictionary<DBIndex, IndexedObjectType> newTable = new Dictionary<DBIndex, IndexedObjectType>();

			List<IndexedObjectType> newAssets = new List<IndexedObjectType>();
			DBIndex maxIndex = new DBIndex(0);

			foreach (IndexedObjectType asset in FindAssets())
			{
				// Abstract elements will not appear in the table
				if (asset.DatabaseIdentity.IsAbstract)
					continue;

				DBIndex index = asset.DatabaseIdentity.DatabaseIndex;

				if (index != DBIndex.Invalid)
				{
					// Attempt to reslot in at same index
					if (!newTable.ContainsKey(index))
					{
						newTable.Add(index, asset);

						if (index > maxIndex)
							maxIndex = index;

						continue;
					}
					else
						Assert.FailFormat("Found multiple index entries {0} and {1}", asset.DatabaseIdentity, newTable[index]);
				}

				// Could not reslot or needs new ID
				newAssets.Add(asset);
			}

			// Next free index slot (Reuse old freed up indices)
			DBIndex NextFreeIndex(DBIndex idx)
			{
				while (true)
				{
					if (!newTable.ContainsKey(idx))
						break;

					idx = new DBIndex(idx.Offset + 1);
				}

				if (idx > maxIndex)
					maxIndex = idx;

				return idx;
			}

			// Now we've fill in all the existing, add new assets
			DBIndex freeIndex = new DBIndex(0);
			foreach (IndexedObjectType asset in newAssets)
			{
				freeIndex = NextFreeIndex(freeIndex);
				asset.DatabaseIdentity.DatabaseIndex = freeIndex;
				newTable.Add(freeIndex, asset);
			}

			// Cleanup any old values
			if (m_ObjectTable != null)
			{
				foreach (IndexedObjectType obj in m_ObjectTable.Where((obj) => obj != null && !newTable.ContainsValue(obj)))
				{
					obj.DatabaseIdentity.DatabaseIndex = DBIndex.Invalid;
					EditorUtility.SetDirty(obj);
				}
			}

			// Convert new lookup to table
			if (newTable.Count != 0)
			{
				m_ObjectTable = new IndexedObjectType[maxIndex.Offset + 1];

				foreach (var kvp in newTable)
				{
					m_ObjectTable[kvp.Key.Offset] = kvp.Value;
					EditorUtility.SetDirty(kvp.Value);
				}
			}
			else
				m_ObjectTable = new IndexedObjectType[0];
		}

		public void ClearTable()
		{
			foreach (IndexedObjectType obj in m_ObjectTable)
			{
				if (obj != null)
				{
					obj.DatabaseIdentity.DatabaseIndex = DBIndex.Invalid;
					EditorUtility.SetDirty(obj);
				}
			}

			m_ObjectTable = new IndexedObjectType[0];
		}

		/// <summary>
		/// Find all assets to be inserted into this table
		/// </summary>
		protected abstract List<IndexedObjectType> FindAssets();

		/// <summary>
		/// Should the asset at this path be checked
		/// </summary>
		protected bool IsValidAssetPath(string path)
		{
			return path.StartsWith(m_SearchDirectory, System.StringComparison.CurrentCultureIgnoreCase);
		}
#endif

		public void OnBeforeSerialize()
		{
			if (m_ObjectTable == null)
			{
				m_SerializedTable = null;
			}
			else
			{
				m_SerializedTable = new Object[m_ObjectTable.Length];

				for (int i = 0; i < m_SerializedTable.Length; ++i)
					m_SerializedTable[i] = m_ObjectTable[i];
			}
		}

		public void OnAfterDeserialize()
		{
			if (m_SerializedTable != null)
			{
				m_ObjectTable = new IndexedObjectType[m_SerializedTable.Length];

				for (int i = 0; i < m_SerializedTable.Length; ++i)
					m_ObjectTable[i] = m_SerializedTable[i] as IndexedObjectType;
			}
		}
	}
}