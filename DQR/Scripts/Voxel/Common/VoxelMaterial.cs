using DQR.Debug;
using DQR.Types;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DQR.Voxel.Common
{
	[System.Serializable]
	public class VoxelMaterial : ISerializationCallbackReceiver
	{
		[System.Serializable]
		private class FaceProperties
		{
			public VoxelFaces AffectedFaces = VoxelFaces.All;
			public VoxelMaterialProperties MaterialProperties = default;
			public WeightedCollection<GameObject> FaceDressing = default;
		}

		[SerializeField]
		private FaceProperties[] m_Properties = null;

		private Dictionary<VoxelFace, FaceProperties> m_FaceProperties = null;
		
		public VoxelMaterial()
		{
		}

		public static bool ShouldGenerateFaceBetween(VoxelMaterial a, VoxelMaterial b)
		{
			if (a != null && b != null)
				return a.Properties.IsOpaque != b.Properties.IsOpaque;

			// One of or both are null if here, so assume one is empty
			else if (a != null || b != null)
				return true;
			
			return false;
		}

		private FaceProperties GetFaceProperties(VoxelFace face)
		{
			if (m_FaceProperties.TryGetValue(face, out FaceProperties props))
				return props;

			Assert.FailFormat("Failed to get properties for face '{0}'", face);
			return m_FaceProperties[m_FaceProperties.Keys.FirstOrDefault()];
		}

		public static VoxelMaterial Lerp(VoxelMaterial a, VoxelMaterial b, float t)
		{
			VoxelMaterial output = new VoxelMaterial();

			foreach (var face in VoxelFaces.All.ToFaceCollection())
			{
				FaceProperties outputFace = new FaceProperties();
				var propsA = a.GetFaceProperties(face);
				var propsB = b.GetFaceProperties(face);

				outputFace.AffectedFaces = face.ToFaces();
				outputFace.MaterialProperties = VoxelMaterialProperties.Lerp(propsA.MaterialProperties, propsB.MaterialProperties, t);
				outputFace.FaceDressing = WeightedCollection<GameObject>.Lerp(propsA.FaceDressing, propsB.FaceDressing, t);
				
				output.m_FaceProperties.Add(face, outputFace);
			}
			
			return output;
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			m_FaceProperties = new Dictionary<VoxelFace, FaceProperties>();

			foreach (var prop in m_Properties)
			{
				foreach (var face in prop.AffectedFaces.ToFaceCollection())
					m_FaceProperties[face] = prop;
			}
		}
	}
}
