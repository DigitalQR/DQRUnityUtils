using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DQR.Voxel
{
	public struct VoxelVertexInput
	{
		public IVoxelVolume SourceVolume;
		public Vector3Int Coord;
		public Vector3 CoordOffset;
		public Vector3Int Normal;
		public VoxelCell Cell;
	}

	public struct VoxelVertexOutput
	{
		public Material RenderedMaterial;
		public Vector3 Position;
		public Vector3 Normal;
		public Vector4[] UVs;

		public void SetDefaults(VoxelVertexInput input, Material mat, VoxelModelGenerationSettings settings)
		{
			BoundsInt bounds = input.SourceVolume.GetVolumeBounds();
			Vector3 centre = Vector3.Scale(bounds.size, settings.NormalizedPivot) + bounds.min;

			RenderedMaterial = mat;
			Position = Vector3.Scale(((input.Coord + input.CoordOffset) - centre), settings.Scale);
			Normal = input.Normal;
		}
	}
	
	public interface IVoxelMaterialResolver
	{
		bool ShouldAddFace(Vector3Int fromCoord, VoxelCell fromCell, Vector3Int toCoord, VoxelCell toCell, out object scratch);
		VoxelVertexOutput ResolveVoxelVertex(VoxelVertexInput input, object scratch, VoxelModelGenerationSettings settings);
	}
}
