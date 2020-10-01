using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Voxel.Common
{
	[CreateAssetMenu(menuName = "DQR/Voxel/New Voxel Brush")]
	[System.Serializable]
	public class VoxelBrush : ScriptableObject
	{
		[SerializeField]
		private VoxelMaterial m_Material = null;

		[Header("Brush Settings")]
		[SerializeField, Range(0.0f, 1.0f)]
		private float m_Strength = 1.0f;

		[SerializeField, Min(0.0f)]
		private float m_Radius = 1.0f;

		[SerializeField, Min(0.0f)]
		private float m_Falloff;

		public VoxelMaterial Material
		{
			get => m_Material;
		}
	}
}
