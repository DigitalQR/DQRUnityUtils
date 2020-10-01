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
	/// Collects all indexed scriptable objects found in assets
	/// </summary>
	[CreateAssetMenu(menuName = "DQR/Database/New ScriptableObject DB")]
	[System.Serializable]
	public class IndexScriptableTable : IndexedTable<IndexedScriptableObject>
	{
		[SerializeField, SubTypeOf(typeof(IndexedScriptableObject))]
		private SerializableType m_ObjectType = new SerializableType(typeof(IndexedScriptableObject));

		public Type GetAt<Type>(DBIndex index) where Type : IndexedScriptableObject
		{
			IndexedScriptableObject obj = GetAt(index);
			if (obj != null)
				return obj as Type;
			return null;
		}

#if UNITY_EDITOR
		protected override List<IndexedScriptableObject> FindAssets()
		{
			string[] assetPaths = AssetDatabase.GetAllAssetPaths();
			List<IndexedScriptableObject> entries = new List<IndexedScriptableObject>();

			foreach (string assetPath in assetPaths)
			{
				if (IsValidAssetPath(assetPath))
				{
					IndexedScriptableObject entry = AssetDatabase.LoadAssetAtPath<IndexedScriptableObject>(assetPath);

					if (entry != null && m_ObjectType.IsAssignableFrom(entry.GetType()))
						entries.Add(entry);
				}
			}

			return entries;
		}
#endif
	}
}