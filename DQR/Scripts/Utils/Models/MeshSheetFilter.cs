using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Models
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	public class MeshSheetFilter : MonoBehaviour
	{
		[SerializeField]
		private MeshSheet m_MeshSheet = null;

		private float m_Timer;

		private MeshFilter m_CachedFilter;
		private MeshFilter CachedFilter
		{
			get
			{
				if (m_CachedFilter == null)
					m_CachedFilter = GetComponent<MeshFilter>();

				return m_CachedFilter;
			}
		}

		private void Update()
		{
			MeshFilter filter = CachedFilter;

			if (filter)
			{
				if (m_MeshSheet == null)
				{
					filter.mesh = null;
				}
				else
				{
#if UNITY_EDITOR
					if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
					{
						m_Timer = 0.0f;
						filter.mesh = m_MeshSheet.GetMeshAtFrame(0);
						return;
					}
#endif

					m_Timer += Time.deltaTime;
					filter.mesh = m_MeshSheet.GetMeshAtTime(m_Timer);
				}
			}
		}
	}
}