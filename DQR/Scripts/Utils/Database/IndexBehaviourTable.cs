using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
#endif

using DQR.Types;

namespace DQR.Database
{
	/// <summary>
	/// Collects all prefabs containing indexed behaviours
	/// </summary>
	[CreateAssetMenu(menuName = "DQR/Database/New MonoBehaviour DB")]
	[System.Serializable]
	public class IndexBehaviourTable : IndexedTable<IndexedBehaviour>
	{
		[SerializeField, SubTypeOf(typeof(IndexedBehaviour))]
		private SerializableType m_ObjectType = new SerializableType(typeof(IndexedBehaviour));

		public Type GetAt<Type>(DBIndex index) where Type : IndexedBehaviour
		{
			IndexedBehaviour obj = GetAt(index);
			if (obj != null)
				return obj as Type;
			return null;
		}

#if UNITY_EDITOR
		protected override List<IndexedBehaviour> FindAssets()
		{
			string[] assetPaths = AssetDatabase.GetAllAssetPaths();
			List<IndexedBehaviour> entries = new List<IndexedBehaviour>();

			foreach (string assetPath in assetPaths)
			{
				if (IsValidAssetPath(assetPath))
				{
					IndexedBehaviour entry = AssetDatabase.LoadAssetAtPath<IndexedBehaviour>(assetPath);

					if (entry != null && m_ObjectType.IsAssignableFrom(entry.GetType()))
						entries.Add(entry);
				}
			}

			return entries;
		}
#endif
	}
}