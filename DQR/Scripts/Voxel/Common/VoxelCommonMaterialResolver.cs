using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Voxel.Common
{
	public class VoxelCommonMaterialResolver : IVoxelMaterialResolver
	{
		private List<VoxelMaterial> m_MaterialTable;
		private Dictionary<VoxelMaterial, int> m_MaterialIndexLookup;

		public VoxelCommonMaterialResolver()
		{
			m_MaterialTable = new List<VoxelMaterial>();
			m_MaterialIndexLookup = new Dictionary<VoxelMaterial, int>();
		}

		public VoxelMaterial VoxelCellToMaterial(VoxelCell cell)
		{
			if (cell != VoxelCell.Invalid)
			{
				int index = cell.m_int32 - 1;

				if (index >= 0 && index < m_MaterialTable.Count)
				{
					return m_MaterialTable[index];
				}
				else
					Assert.FailFormat("Material index '{0}' out of range (0, {1})", index, m_MaterialTable.Count);
			}

			return null;
		}

		public VoxelCell VoxelMaterialToCell(VoxelMaterial mat)
		{
			int index;
			if (!m_MaterialIndexLookup.TryGetValue(mat, out index))
			{
				index = m_MaterialTable.Count;
				m_MaterialTable.Add(mat);

				m_MaterialIndexLookup.Add(mat, index);
			}

			return new VoxelCell(index + 1);
		}


		public VoxelMaterial GetVoxelMaterial(IVoxelVolume volume, int x, int y, int z)
		{
			return VoxelCellToMaterial(volume.GetVoxelCell(x, y, z));
		}

		public VoxelMaterial GetVoxelMaterial(IVoxelVolume volume, Vector3Int coord)
		{
			return VoxelCellToMaterial(volume.GetVoxelCell(coord));
		}

		public bool SetVoxelMaterial(IVoxelVolume volume, int x, int y, int z, VoxelMaterial material)
		{
			return volume.SetVoxelCell(x, y, z, VoxelMaterialToCell(material));
		}

		public bool SetVoxelMaterial(IVoxelVolume volume, Vector3Int coord, VoxelMaterial material)
		{
			return volume.SetVoxelCell(coord, VoxelMaterialToCell(material));
		}


		public bool BlendVoxelMaterial(IVoxelVolume volume, int x, int y, int z, VoxelMaterial material, float strength)
		{
			// TODO - May need to optimize this to prevent blends being created excessively
			VoxelMaterial baseMaterial = GetVoxelMaterial(volume, x, y, z);
			VoxelMaterial blendedMaterial = VoxelMaterial.Lerp(baseMaterial, material, strength);

			return volume.SetVoxelCell(x, y, z, VoxelMaterialToCell(blendedMaterial));
		}

		public bool BlendVoxelMaterial(IVoxelVolume volume, Vector3Int coord, VoxelMaterial material, float strength)
		{
			return BlendVoxelMaterial(volume, coord.x, coord.y, coord.z, material, strength);
		}


		public VoxelVertexOutput ResolveVoxelVertex(VoxelVertexInput input, VoxelModelGenerationSettings settings)
		{
			VoxelVertexOutput output = new VoxelVertexOutput();
			output.SetDefaults(input, settings);

			VoxelMaterial mat = VoxelCellToMaterial(input.Cell);
			Assert.Message(mat != null, "Invalid material referenced by cell");

			if (mat != null)
			{
				mat.ApplyToVertex(input, ref output);
			}
			
			return output;
		}

		public bool ShouldAddFace(Vector3Int fromCoord, VoxelCell fromCell, Vector3Int toCoord, VoxelCell toCell)
		{
			return VoxelMaterial.ShouldGenerateFaceBetween(VoxelFaceHelpers.NormalToFace(toCoord - fromCoord), VoxelCellToMaterial(fromCell), VoxelCellToMaterial(toCell));
		}

		public bool ShouldConsiderForModel(Vector3Int coord, VoxelCell cell)
		{
			return cell != VoxelCell.Invalid;
		}
	}
}