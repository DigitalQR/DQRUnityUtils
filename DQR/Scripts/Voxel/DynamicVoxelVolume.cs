using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Voxel
{
	public class DynamicVoxelVolume : IVoxelVolume
	{
		private Dictionary<Vector3Int, VoxelCell> m_Data;
		private BoundsInt m_Bounds;
		private VoxelCell m_DefaultMissingValue;

		public DynamicVoxelVolume()
		{
			m_Data = new Dictionary<Vector3Int, VoxelCell>();
			m_DefaultMissingValue = VoxelCell.Invalid;
		}

		public DynamicVoxelVolume(VoxelCell missingValue)
		{
			m_Data = new Dictionary<Vector3Int, VoxelCell>();
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
			m_Data[coord] = cell;

			if (m_Data.Count == 0)
				m_Bounds.SetMinMax(coord, coord);
			else
			{
				m_Bounds.min = Vector3Int.Min(m_Bounds.min, coord);
				m_Bounds.max = Vector3Int.Max(m_Bounds.max, coord);
			}

			return true;
		}
	}
}