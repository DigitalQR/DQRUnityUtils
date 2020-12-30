using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Voxel.Model
{
	[CreateAssetMenu(menuName = "DQR/Voxel/New Voxel Material Surface")]
	[System.Serializable]
	public class VoxelMaterialSurface : ScriptableObject, ISerializationCallbackReceiver
	{
		[System.Serializable]
		private class FaceProperties
		{
			public VoxelFaces AffectedFaces = VoxelFaces.All;
			public VoxelMaterial Material = default;
		}

		[SerializeField]
		private FaceProperties[] m_Properties = null;

		private Dictionary<VoxelFace, FaceProperties> m_FaceProperties = null;
		
		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			m_FaceProperties = new Dictionary<VoxelFace, FaceProperties>();

			if (m_Properties != null && m_Properties.Length != 0)
			{
				// Default ever face to whatever the default is
				foreach (var face in VoxelFaces.All.ToFaceCollection())
					m_FaceProperties.Add(face, m_Properties[0]);

				foreach (var props in m_Properties)
				{
					foreach (var face in props.AffectedFaces.ToFaceCollection())
						m_FaceProperties[face] = props;
				}
			}
		}

		public VoxelMaterial GetMaterialForFace(VoxelFace face)
		{
			return m_FaceProperties[face].Material;
		}
	}
}
