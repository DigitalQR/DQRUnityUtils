using DQR.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Voxel.Common
{
	[System.Serializable]
	public class VoxelMaterial
	{
		public VoxelMaterialBasic Properties;

		public SerializableDictionary<VoxelFaces, GameObject> FaceDressing;

		public static bool ShouldGenerateFaceBetween(VoxelMaterial a, VoxelMaterial b)
		{
			if (a != null && b != null)
				return a.Properties.IsOpaque != b.Properties.IsOpaque;

			// One of or both are null if here, so assume one is empty
			else if (a != null || b != null)
				return true;
			
			return false;
		}
		
	}
}
