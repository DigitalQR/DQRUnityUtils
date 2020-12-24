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
		public int SubmeshID;
		public Vector3 Position;
		public Vector3 Normal;
		public Color Colour;
		public Vector4[] UVs;

		public void SetDefaults(VoxelVertexInput input, VoxelModelGenerationSettings settings)
		{
			SubmeshID = 0;

			BoundsInt bounds = input.SourceVolume.GetVolumeBounds();
			Vector3 centre = Vector3.Scale(bounds.size, settings.NormalizedPivot) + bounds.min;

			Position = Vector3.Scale(((input.Coord + input.CoordOffset) - centre), settings.Scale);
			Normal = input.Normal;
		}
	}

	public interface IVoxelMaterialResolver
	{
		bool ShouldConsiderForModel(Vector3Int coord, VoxelCell cell);
		bool ShouldAddFace(Vector3Int fromCoord, VoxelCell fromCell, Vector3Int toCoord, VoxelCell toCell);
		VoxelVertexOutput ResolveVoxelVertex(VoxelVertexInput input, VoxelModelGenerationSettings settings);
	}

	public class DefaultVoxelMaterialResolver : IVoxelMaterialResolver
	{
		public VoxelVertexOutput ResolveVoxelVertex(VoxelVertexInput input, VoxelModelGenerationSettings settings)
		{
			VoxelVertexOutput output = new VoxelVertexOutput();
			output.SetDefaults(input, settings);
			return output;
		}

		public bool ShouldAddFace(Vector3Int fromCoord, VoxelCell fromCell, Vector3Int toCoord, VoxelCell toCell)
		{
			return fromCell != toCell;
		}

		public bool ShouldConsiderForModel(Vector3Int coord, VoxelCell cell)
		{
			return cell != VoxelCell.Invalid;
		}
	}
}
