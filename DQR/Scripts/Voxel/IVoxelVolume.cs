using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DQR.Debug;

namespace DQR.Voxel
{
	public interface IVoxelVolume
	{
		VoxelCell GetVoxelCell(int x, int y, int z);
		bool SetVoxelCell(int x, int y, int z, VoxelCell cell);

		bool IsVolumeReadable();
		bool IsVolumeWriteable();

		BoundsInt GetVolumeBounds();
	}

	public static class VoxelVolumeHelpers
	{
		public static VoxelCell GetVoxelCell(this IVoxelVolume volume, Vector3Int coord)
		{
			return volume.GetVoxelCell(coord.x, coord.y, coord.z);
		}

		public static bool SetVoxelCell(this IVoxelVolume volume, Vector3Int coord, VoxelCell cell)
		{
			return volume.SetVoxelCell(coord.x, coord.y, coord.z, cell);
		}

		public static bool IsValidCoord(this IVoxelVolume volume, int x, int y, int z)
		{
			return volume.IsValidCoord(new Vector3Int(x,y,z));
		}

		public static bool IsValidCoord(this IVoxelVolume volume, Vector3Int coord)
		{
			BoundsInt bounds = volume.GetVolumeBounds();
			Vector3Int clammpedCoord = Vector3Int.Min(Vector3Int.Max(coord, bounds.min), bounds.max);
			return clammpedCoord == coord;
		}

		public static bool TryGetVoxelCell(this IVoxelVolume volume, int x, int y, int z, out VoxelCell value)
		{
			Assert.Message(volume.IsVolumeReadable(), "TryGetVoxelCell being called on non-readable volume");
			if (volume.IsValidCoord(x, y, z))
			{
				value = volume.GetVoxelCell(x, y, z);
				return true;
			}

			value = VoxelCell.Invalid;
			return false;
		}

		public static bool TryGetVoxelCell(this IVoxelVolume volume, Vector3Int coord, out VoxelCell value)
		{
			Assert.Message(volume.IsVolumeReadable(), "TryGetVoxelCell being called on non-readable volume");
			if (volume.IsValidCoord(coord))
			{
				value = volume.GetVoxelCell(coord.x, coord.y, coord.z);
				return true;
			}

			value = VoxelCell.Invalid;
			return false;
		}
	}
}
