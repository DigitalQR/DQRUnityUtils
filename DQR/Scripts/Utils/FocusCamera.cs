using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR
{
	public class FocusCamera : MonoBehaviour
	{
		[SerializeField]
		private Transform m_CurrentFocus = null;

		[SerializeField]
		private Transform m_FocusAnchor = null;

		[Header("Animation")]
		[SerializeField]
		private bool m_PreferSlerp = false;

		[SerializeField]
		private float m_Snappiness = 1.0f;

		[SerializeField]
		private float m_MinTeleportDistance = 20.0f;

		public Transform FocusAnchor
		{
			get => m_FocusAnchor ? m_FocusAnchor : transform;
		}

		public Transform CurrentFocus
		{
			get => m_CurrentFocus;
			set => m_CurrentFocus = value;
		}
		
		public Vector3 CurrentFocusPosition
		{
			get => m_CurrentFocus ? m_CurrentFocus.position : FocusAnchor.position;
		}

		private void LateUpdate()
		{			
			if (Vector3.Distance(FocusAnchor.position, CurrentFocusPosition) >= m_MinTeleportDistance)
			{
				transform.position = CurrentFocusPosition;
			}
			else
			{
				// Offset all positions by anchor delta
				Vector3 anchorDelta = FocusAnchor.position - transform.position;

				Vector3 currentPosition = transform.position;
				Vector3 targetPosition = CurrentFocusPosition - anchorDelta;

				float t = Time.deltaTime * m_Snappiness;
				if (m_PreferSlerp)
					transform.position = Vector3.Slerp(currentPosition, targetPosition, t);
				else
					transform.position = Vector3.Lerp(currentPosition, targetPosition, t);
			}
		}
	}
}
