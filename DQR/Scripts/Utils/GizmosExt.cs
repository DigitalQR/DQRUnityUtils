using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR
{
	public static class GizmosExt
	{
		public static void DrawSquare(Vector3 position, Vector3 normal, Vector2 extents)
		{
			Vector2 halfExtents = extents * 0.5f;
			Vector3 right = Vector3.Cross(Vector3.forward, normal);
			Vector3 forward = Vector3.Cross(right, normal);

			Gizmos.DrawLine(position + right * halfExtents.x + forward * halfExtents.y, position - right * halfExtents.x + forward * halfExtents.y);
			Gizmos.DrawLine(position + right * halfExtents.x - forward * halfExtents.y, position - right * halfExtents.x - forward * halfExtents.y);

			Gizmos.DrawLine(position + right * halfExtents.x + forward * halfExtents.y, position + right * halfExtents.x - forward * halfExtents.y);
			Gizmos.DrawLine(position - right * halfExtents.x + forward * halfExtents.y, position - right * halfExtents.x - forward * halfExtents.y);
		}

		public static void DrawArc(Vector3 position, Vector3 direction, Vector3 up, float angle)
		{
			int steps = Mathf.Max(2, Mathf.CeilToInt(angle / 10.0f));

			float baseAngle = angle * -0.5f;
			float angleDelta = angle / steps;

			for (int i = 0; i < steps; ++i)
			{
				Vector3 posA = position + Quaternion.AngleAxis(baseAngle + angleDelta * (i + 0), up) * direction;
				Vector3 posB = position + Quaternion.AngleAxis(baseAngle + angleDelta * (i + 1), up) * direction;
				Gizmos.DrawLine(posA, posB);

				if (i == 0)
					Gizmos.DrawLine(position, posA);

				if (i == steps - 1)
					Gizmos.DrawLine(position, posB);
			}
		}
	}
}
