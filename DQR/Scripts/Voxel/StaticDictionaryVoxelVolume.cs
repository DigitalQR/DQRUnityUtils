using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Voxel
{
	public class StaticDictionaryVoxelVolume : IVoxelVolume
	{
		private Dictionary<Vector3Int, VoxelCell> m_Data;
		private BoundsInt m_Bounds;
		private VoxelCell m_DefaultMissingValue;

		public StaticDictionaryVoxelVolume(BoundsInt bounds)
		{
			m_Data = new Dictionary<Vector3Int, VoxelCell>();
			m_Bounds = bounds;
			m_DefaultMissingValue = VoxelCell.Invalid;
		}

		public StaticDictionaryVoxelVolume(BoundsInt bounds, VoxelCell missingValue)
		{
			m_Data = new Dictionary<Vector3Int, VoxelCell>();
			m_Bounds = bounds;
			m_DefaultMissingValue = missingValue;
		}

		public BoundsInt GetVolumeBounds()
		{
			return m_Bounds;
		}

		public bool IsVolumeReadable()
		{
			return true;
		}

		public bool IsVolumeWriteable()
		{
			return true;
		}

		public VoxelCell GetVoxelCell(int x, int y, int z)
		{
			if (m_Data.TryGetValue(new Vector3Int(x, y, z), out VoxelCell cell))
				return cell;

			return m_DefaultMissingValue;
		}

		public bool SetVoxelCell(int x, int y, int z, VoxelCell cell)
		{
			Vector3Int coord = new Vector3Int(x, y, z);
			if (m_Bounds.Contains(coord))
			{
				m_Data[coord] = cell;
				return true;
			}

			return false;
		}
	}
}