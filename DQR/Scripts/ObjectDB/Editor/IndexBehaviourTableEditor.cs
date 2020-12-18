#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DQR.Database
{
	[CustomEditor(typeof(IndexBehaviourTable))]
	[CanEditMultipleObjects]
	public class IndexBehaviourTableEditor : IndexedTableHelper<IndexedBehaviour>
	{
		public override void OnInspectorGUI()
		{
			if (targets.Length == 1)
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ObjectType"));

			base.OnInspectorGUI();
		}
	}
}
#endif