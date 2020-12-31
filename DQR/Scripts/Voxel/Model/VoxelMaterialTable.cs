using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DQR.Voxel.Model
{
	public class VoxelMaterialTable
	{
		private List<VoxelMaterial> m_MaterialTable;
		private Dictionary<VoxelMaterial, int> m_MaterialIndexLookup;

		private Texture2D m_MappingTexture = null;
		private bool m_MappingTextureDirty = true;

		public VoxelMaterialTable()
		{
			m_MaterialTable = new List<VoxelMaterial>();
			m_MaterialIndexLookup = new Dictionary<VoxelMaterial, int>();
		}

		public IEnumerable<VoxelMaterial> AllResources
		{
			get => m_MaterialTable;
		}

		public int GetMaterialIndex(VoxelMaterial res)
		{
			int index;
			if (!m_MaterialIndexLookup.TryGetValue(res, out index))
			{
				index = m_MaterialTable.Count;
				m_MaterialTable.Add(res);

				m_MaterialIndexLookup.Add(res, index);
				m_MappingTextureDirty = true;
			}

			return index;
		}

		public Texture2D GenerateMaterialTexture()
		{
			if (m_MappingTextureDirty)
			{
				GenerateMaterialTextureInternal();
				m_MappingTextureDirty = false;
			}

			return m_MappingTexture;
		}

		private Texture2D GenerateMaterialTextureInternal()
		{
			int maxChannel = m_MaterialTable.Select((m) => m.MaterialData.Length).Aggregate((a, b) => Mathf.Max(a, b));

			m_MappingTexture = new Texture2D(m_MaterialTable.Count, Mathf.CeilToInt(maxChannel / 4.0f), TextureFormat.RGBA32, false);
			//m_MappingTexture.alphaIsTransparency = false;
			m_MappingTexture.filterMode = FilterMode.Point;

			int i = 0;
			foreach (var curMat in m_MaterialTable)
			{
				int y = 0;
				for (int ii = 0; ii < maxChannel; ++y)
				{
					float GetValue(int j)
					{
						return j < curMat.MaterialData.Length ? curMat.MaterialData[j] : 0.0f;
					};

					Vector4 value = new Vector4(
						GetValue(ii++),
						GetValue(ii++),
						GetValue(ii++),
						GetValue(ii++)
					);
					m_MappingTexture.SetPixel(i, y, value);
				}

				++i;
			}

			m_MappingTexture.Apply(false, true);
			return m_MappingTexture;
		}
	}
}
