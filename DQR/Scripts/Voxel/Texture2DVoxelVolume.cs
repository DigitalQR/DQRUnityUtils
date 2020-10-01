using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Voxel
{
	public class Texture2DVoxelVolume : IVoxelVolume
	{
		private Texture2D m_Texture;

		public Texture2DVoxelVolume(Texture2D texture)
		{
			m_Texture = texture;
		}

		public BoundsInt GetVolumeBounds()
		{
			BoundsInt bounds = new BoundsInt();
			bounds.min = new Vector3Int(0, 0, 0);
			bounds.max = new Vector3Int(m_Texture.width, m_Texture.height, 0);
			return bounds;
		}

		public VoxelCell GetVoxelCell(int x, int y, int z)
		{
			VoxelCell cell = new VoxelCell(0);
			if (z == 0 && x >= 0 && x < m_Texture.width && y >= 0 && y < m_Texture.height)
			{
				Color32 pixel = m_Texture.GetPixel(x, y);
				cell.m_uint8_0 = pixel.r;
				cell.m_uint8_1 = pixel.g;
				cell.m_uint8_2 = pixel.b;
				cell.m_uint8_3 = pixel.a;
			}
			return cell;
		}

		public bool IsVolumeReadable()
		{
			return m_Texture.isReadable;
		}

		public bool IsVolumeWriteable()
		{
			return false;
		}

		public bool SetVoxelCell(int x, int y, int z, VoxelCell cell)
		{
			throw new System.NotImplementedException();
		}
	}
}