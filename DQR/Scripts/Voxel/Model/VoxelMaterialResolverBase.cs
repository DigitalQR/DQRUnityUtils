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
		public abstract VoxelMaterial GetMaterialForInput(Vector3Int fromCoord, VoxelCell fromCell, Vector3Int toCoord, VoxelCell toCell);

		protected static Texture2D GenerateMaterialTexture(ResourceMappedCellTable<VoxelMaterial> mapping)
		{
			var mats = mapping.AllResources.ToArray();
			int maxChannel = mats.Select((m) => m.MaterialData.Length).Aggregate((a, b) => Mathf.Max(a, b));

			Texture2D tex = new Texture2D(mats.Length, Mathf.CeilToInt(maxChannel / 4.0f), TextureFormat.RGBA32, false);
			//tex.alphaIsTransparency = false;
			tex.filterMode = FilterMode.Point;
			
			for (int i = 0; i < mats.Length; ++i)
			{
				var curMat = mats[i];

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
					tex.SetPixel(i, y, value);
				}
			}

			tex.Apply(false, true);
			return tex;
		}

		public bool ShouldAddFace(Vector3Int fromCoord, VoxelCell fromCell, Vector3Int toCoord, VoxelCell toCell, out object scratch)
		{
			var fromMat = GetMaterialForInput(fromCoord, fromCell, toCoord, toCell);
			var toMat = GetMaterialForInput(toCoord, toCell, fromCoord, fromCell);
			
			scratch = new VoxelMaterialBaseVertexScratch(fromMat);
			return fromMat != null && toMat == null;
		}

		public VoxelVertexOutput ResolveVoxelVertex(VoxelVertexInput input, object scratch, VoxelModelGenerationSettings settings)
		{
			VoxelMaterial material = (scratch as VoxelMaterialBaseVertexScratch).Material;
			VoxelVertexOutput output = new VoxelVertexOutput();

			output.SetDefaults(input, material.RenderMaterial, settings);

			int quadSize = Mathf.CeilToInt(material.MaterialData.Length / 4.0f);
			output.UVs = new Vector4[1 + quadSize];

			float GetMaterialData(int index)
			{
				return index < material.MaterialData.Length ? material.MaterialData[index] : 0.0f;
			};

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

			for (int i = 0; i < output.UVs.Length - 1; ++i)
			{
				output.UVs[i + 1] = new Vector4(
					GetMaterialData(i * 4 + 0),
					GetMaterialData(i * 4 + 1),
					GetMaterialData(i * 4 + 2),
					GetMaterialData(i * 4 + 3)
				);
			}

			return output;
		}
	}
}
