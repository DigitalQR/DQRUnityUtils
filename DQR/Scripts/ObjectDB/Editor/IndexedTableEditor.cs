#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DQR.Database
{
	public class IndexedTableHelper<IndexedObjectType> : Editor where IndexedObjectType : Object, IDatabaseIndexable
	{
		protected IndexedTable<IndexedObjectType> m_CurrentDB;
		protected IndexedTable<IndexedObjectType>[] m_CurrentDBs;
		private bool m_ExpandElements;

		private void OnEnable()
		{
			m_CurrentDB = target as IndexedTable<IndexedObjectType>;
		}

		public override void OnInspectorGUI()
		{
			if (targets.Length == 1)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SearchDirectory"));

				if (GUILayout.Button("Refresh"))
				{
					m_CurrentDB.RefreshTable();
					EditorUtility.SetDirty(m_CurrentDB);
				}

				if (GUILayout.Button("Clear"))
				{
					m_CurrentDB.ClearTable();
					EditorUtility.SetDirty(m_CurrentDB);
				}

				if ((m_ExpandElements = EditorGUILayout.Foldout(m_ExpandElements, "(Discovered " + m_CurrentDB.Count + " entries)")))
				{
					for (int i = 0; i < m_CurrentDB.Count; ++i)
					{
						DBIndex dbIndex = new DBIndex((uint)i);
						var entry = m_CurrentDB.GetAt(dbIndex);

						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField(i + "", GUILayout.Width(20));
						EditorGUI.BeginDisabledGroup(true);

						if (entry != null)
							EditorGUILayout.ObjectField(entry, entry.GetType(), false);
						else
							EditorGUILayout.LabelField("<Unassigned>");

						EditorGUI.EndDisabledGroup();
						EditorGUILayout.EndHorizontal();
					}
				}
			}
			else
			{
				if (GUILayout.Button("Refresh All"))
				{
					foreach (Object currTarget in targets)
					{
						IndexedTable<IndexedObjectType> currDB = currTarget as IndexedTable<IndexedObjectType>;
						if (currDB != null)
						{
							currDB.RefreshTable();
							EditorUtility.SetDirty(currDB);
						}
					}
				}

				if (GUILayout.Button("Clear All"))
				{
					foreach (Object currTarget in targets)
					{
						IndexedTable<IndexedObjectType> currDB = currTarget as IndexedTable<IndexedObjectType>;
						if (currDB != null)
						{
							currDB.ClearTable();
							EditorUtility.SetDirty(currDB);
						}
					}
				}

				foreach (Object currTarget in targets)
				{
					IndexedTable<IndexedObjectType> currDB = currTarget as IndexedTable<IndexedObjectType>;
					if (currDB != null)
						EditorGUILayout.LabelField("('" + currDB.name + "' Discovered " + currDB.Count + " entries)");
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif