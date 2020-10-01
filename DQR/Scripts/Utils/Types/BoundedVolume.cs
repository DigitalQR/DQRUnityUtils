using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DQR.Types
{
	public class BoundedVolume : MonoBehaviour
	{
		[SerializeField]
		private Bounds m_LocalBounds = new Bounds();

		public Bounds WorldBounds
		{
			get => new Bounds(m_LocalBounds.center + transform.position, Vector3.Scale(m_LocalBounds.size, transform.lossyScale));
			set => m_LocalBounds = new Bounds(value.center - transform.position, value.size);
		}

		public Bounds LocalBounds
		{
			get => m_LocalBounds;
			set => m_LocalBounds = value;
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmos()
		{
			Gizmos.color = new Color(1, 1, 0);
			Gizmos.DrawWireCube(WorldBounds.center, WorldBounds.size);
		}

		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.color = new Color(1, 1, 0, 0.2f);
			Gizmos.DrawCube(WorldBounds.center, WorldBounds.size);
		}
#endif
	}
}
