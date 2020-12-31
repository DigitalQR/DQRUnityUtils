using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Voxel.Model
{
	[CreateAssetMenu(menuName = "DQR/Voxel/New Voxel Material")]
	[System.Serializable]
	public class VoxelMaterial : ScriptableObject, ISerializationCallbackReceiver
	{
		[SerializeField]
		private VoxelMaterialLayout m_Layout = null;

		[SerializeField]
		private Material m_RenderMaterial = null;

		[SerializeField]
		private float[] m_MaterialData = null;

		public Material RenderMaterial
		{
			get => m_RenderMaterial;
		}

		public float[] MaterialData
		{
			get => m_MaterialData;
		}

		public void OnBeforeSerialize()
		{
			if (m_Layout == null)
			{
				m_MaterialData = null;
			}
			else
			{
				if (m_MaterialData == null || m_Layout.ChannelCount != m_MaterialData.Length)
				{
					float[] oldData = m_MaterialData;
					m_MaterialData = new float[m_Layout.ChannelCount];

					for (int i = 0; i < m_MaterialData.Length; ++i)
					{
						if (oldData != null && i < oldData.Length)
							m_MaterialData[i] = oldData[i];
						else
							m_MaterialData[i] = 0;
					}
				}

				int offset = 0;
				for (int i = 0; i < m_Layout.LayoutCount; ++i)
				{
					var desc = m_Layout.GetLayoutDesc(i);

					for (int ii = 0; ii < desc.DataFormat.GetChannelCount(); ++ii, ++offset)
						m_MaterialData[offset] = desc.ProcessValue(m_MaterialData[offset]);
				}
			}
		}

		public void OnAfterDeserialize()
		{
		}
	}

	public class VoxelMaterialResolver : VoxelMaterialResolverBase
	{
		private ResourceMappedCellTable<VoxelMaterial> m_Table;

		public VoxelMaterialResolver(ResourceMappedCellTable<VoxelMaterial> table)
		{
			m_Table = table;
		}


		public override VoxelMaterial GetMaterialForInput(Vector3Int fromCoord, VoxelCell fromCell, Vector3Int toCoord, VoxelCell toCell)
		{
			return m_Table.CellToResource(fromCell);
		}
	}
}
