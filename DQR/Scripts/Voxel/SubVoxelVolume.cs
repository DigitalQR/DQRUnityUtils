using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Voxel
{
	public class VoxelVolumeSubview : IVoxelVolume
	{
		private IVoxelVolume m_Source;
		private BoundsInt m_Bounds;

		public VoxelVolumeSubview(IVoxelVolume source, Vector3Int offset, Vector3Int size)
		{
			m_Source = source;
			BoundsInt sourceBounds = source.GetVolumeBounds();
			m_Bounds = new BoundsInt();
			m_Bounds.SetMinMax(m_Bounds.min + offset, m_Bounds.min + offset + size);
		}

		public BoundsInt GetVolumeBounds()
		{
			return m_Bounds;
		}

		public VoxelCell GetVoxelCell(int x, int y, int z)
		{
			return m_Source.GetVoxelCell(x, y, z);
		}

		public bool IsVolumeReadable()
		{
			return m_Source.IsVolumeReadable();
		}

		public bool IsVolumeWriteable()
		{
			return m_Source.IsVolumeWriteable();
		}

		public bool SetVoxelCell(int x, int y, int z, VoxelCell cell)
		{
			return m_Source.SetVoxelCell(x, y, z, cell);
		}
	}
}
