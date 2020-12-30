using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Voxel
{
	public class ResourceMappedCellTable<T>
	{
		private List<T> m_ResourceTable;
		private Dictionary<T, int> m_ResourceIndexLookup;

		public ResourceMappedCellTable()
		{
			m_ResourceTable = new List<T>();
			m_ResourceIndexLookup = new Dictionary<T, int>();
		}

		public IEnumerable<T> AllResources
		{
			get => m_ResourceTable;
		}

		public T CellToResource(VoxelCell cell)
		{
			if (cell != VoxelCell.Invalid)
			{
				int index = cell.m_int32 - 1;

				if (index >= 0 && index < m_ResourceTable.Count)
				{
					return m_ResourceTable[index];
				}
				else
					Assert.FailFormat("Resource index '{0}' out of range (0, {1})", index, m_ResourceTable.Count);
			}

			return default;
		}

		public VoxelCell ResourceToCell(T res)
		{
			int index;
			if (!m_ResourceIndexLookup.TryGetValue(res, out index))
			{
				index = m_ResourceTable.Count;
				m_ResourceTable.Add(res);

				m_ResourceIndexLookup.Add(res, index);
			}

			return new VoxelCell(index + 1);
		}
	}

	public class ResourceMappedVoxelVolume<T> : ResourceMappedCellTable<T>, IVoxelVolume
	{
		private IVoxelVolume m_Source;

		public ResourceMappedVoxelVolume(IVoxelVolume source)
		{
			m_Source = source;
		}

		public BoundsInt GetVolumeBounds()
		{
			return m_Source.GetVolumeBounds();
		}
		
		public bool IsVolumeReadable()
		{
			return m_Source.IsVolumeReadable();
		}

		public bool IsVolumeWriteable()
		{
			return m_Source.IsVolumeWriteable();
		}
		
		[System.Obsolete("Use resource mapped variant")]
		public bool SetVoxelCell(int x, int y, int z, VoxelCell cell)
		{
			return m_Source.SetVoxelCell(x, y, z, cell);
		}

		[System.Obsolete("Use resource mapped variant")]
		public VoxelCell GetVoxelCell(int x, int y, int z)
		{
			return m_Source.GetVoxelCell(x, y, z);
		}

		public bool SetResource(int x, int y, int z, T res)
		{
			return m_Source.SetVoxelCell(x, y, z, ResourceToCell(res));
		}

		public bool SetResource(Vector3Int coord, T res)
		{
			return m_Source.SetVoxelCell(coord.x, coord.y, coord.z, ResourceToCell(res));
		}

		public T GetResource(int x, int y, int z)
		{
			return CellToResource(m_Source.GetVoxelCell(x, y, z));
		}

		public T GetResource(Vector3Int coord)
		{
			return CellToResource(m_Source.GetVoxelCell(coord.x, coord.y, coord.z));
		}
	}
}
