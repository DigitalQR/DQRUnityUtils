using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Voxel
{
	public class StaticArrayVoxelVolume : IVoxelVolume
	{
		private VoxelCell[] m_Data;
		private BoundsInt m_Bounds;
		private VoxelCell m_DefaultMissingValue;

		public StaticArrayVoxelVolume(BoundsInt bounds)
		{
			m_Data = new VoxelCell[(bounds.size.x + 1) * (bounds.size.y + 1) * (bounds.size.z + 1)];
			m_Bounds = bounds;
			m_DefaultMissingValue = VoxelCell.Invalid;
		}

		public StaticArrayVoxelVolume(BoundsInt bounds, VoxelCell missingValue)
		{
			m_Data = new VoxelCell[(bounds.size.x + 1) * (bounds.size.y + 1) * (bounds.size.z + 1)];
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

		private int GetArrayIndex(int x, int y, int z)
		{
			int dx = x - m_Bounds.min.x;
			int dy = y - m_Bounds.min.y;
			int dz = z - m_Bounds.min.z;

			return dx + (m_Bounds.size.x + 1) * (dy + (m_Bounds.size.y + 1) * (dz));
		}

		public VoxelCell GetVoxelCell(int x, int y, int z)
		{
			if (this.IsValidCoord(x, y, z))
			{
				int index = GetArrayIndex(x, y, z);
				return m_Data[index];
			}

			return m_DefaultMissingValue;
		}

		public bool SetVoxelCell(int x, int y, int z, VoxelCell cell)
		{
			if (this.IsValidCoord(x, y, z))
			{
				int index = GetArrayIndex(x, y, z);
				m_Data[index] = cell;
				return true;
			}

			return false;
		}
	}
}