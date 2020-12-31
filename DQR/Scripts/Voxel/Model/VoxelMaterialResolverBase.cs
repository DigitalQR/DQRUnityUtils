using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DQR.Voxel.Model
{
	public class VoxelMaterialBaseVertexScratch
	{
		public VoxelMaterial Material = null;

		public VoxelMaterialBaseVertexScratch(VoxelMaterial mat)
		{
			Material = mat;
		}
	}

	public abstract class VoxelMaterialResolverBase : IVoxelMaterialResolver
	{
		protected VoxelMaterialTable m_MaterialTable;

		public VoxelMaterialResolverBase()
		{
			m_MaterialTable = new VoxelMaterialTable();
		}

		public abstract VoxelMaterial GetMaterialForInput(Vector3Int fromCoord, VoxelCell fromCell, Vector3Int toCoord, VoxelCell toCell);

		public virtual bool ShouldGenerateFaceBetween(VoxelMaterial a, VoxelMaterial b)
		{
			return a != null && b == null;
		}

		public Texture2D GenerateMaterialTexture()
		{
			return m_MaterialTable.GenerateMaterialTexture();
		}

		public bool ShouldAddFace(Vector3Int fromCoord, VoxelCell fromCell, Vector3Int toCoord, VoxelCell toCell, out object scratch)
		{
			var fromMat = GetMaterialForInput(fromCoord, fromCell, toCoord, toCell);
			var toMat = GetMaterialForInput(toCoord, toCell, fromCoord, fromCell);
			
			scratch = new VoxelMaterialBaseVertexScratch(fromMat);
			return ShouldGenerateFaceBetween(fromMat, toMat);
		}

		public VoxelVertexOutput ResolveVoxelVertex(VoxelVertexInput input, object scratch, VoxelModelGenerationSettings settings)
		{
			VoxelMaterial material = (scratch as VoxelMaterialBaseVertexScratch).Material;
			VoxelVertexOutput output = new VoxelVertexOutput();

			output.SetDefaults(input, material.RenderMaterial, settings);

			int matId = m_MaterialTable.GetMaterialIndex(material);
			output.UVs = new Vector4[2];
			
			// First channel is reserved
			// Calulcate face UVs (Don't bother clampping)
			Vector3Int absNormal = new Vector3Int(
				Mathf.Abs(input.Normal.x),
				Mathf.Abs(input.Normal.y),
				Mathf.Abs(input.Normal.z)
			);

			Vector3 uvPos = input.Coord + input.CoordOffset;
			Vector2 uvs = new Vector2();
			if (absNormal.x > absNormal.y && absNormal.x > absNormal.z)
				uvs = new Vector2(uvPos.y, uvPos.z);
			else if (absNormal.y > absNormal.x && absNormal.y > absNormal.z)
				uvs = new Vector2(uvPos.x, uvPos.z);
			else
				uvs = new Vector2(uvPos.x, uvPos.y);

			output.UVs[0] = uvs + new Vector2(0.5f, 0.5f);
			output.UVs[1] = new Vector4(matId, 0, 0, 0);
			
			return output;
		}
	}
}
