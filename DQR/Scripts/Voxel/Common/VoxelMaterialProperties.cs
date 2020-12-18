using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DQR.Voxel.Common
{
	[System.Serializable]
	public struct VoxelMaterialProperties : System.IEquatable<VoxelMaterialProperties>
	{
		public Color Colour;

		public Vector4 Tint;

		[Range(0.0f, 1.0f)]
		public float Specular;

		[Range(0.0f, 1.0f)]
		public float Roughness;
		
		public Vector4[] CustomUVs;


		public void SetDefaults()
		{
			Colour = Color.black;
			Tint = Vector4.zero;
			Specular = 0.0f;
			Roughness = 0.5f;
			CustomUVs = null;
		}

		public int CustomUVCount
		{
			get => CustomUVs != null ? CustomUVs.Length : 0;
		}

		public bool IsTransparent
		{
			get => Colour.a < 1.0f;
		}

		public bool IsOpaque
		{
			get => !IsTransparent;
		}

		public static bool ShouldGenerateFaceBetween(VoxelMaterialProperties a, VoxelMaterialProperties b)
		{
			if (a != null && b != null)
				return a.IsOpaque != b.IsOpaque;

			// One of or both are null if here, so assume one is empty
			else if (a != null || b != null)
				return true;

			return false;
		}

		public void ApplyToVertex(VoxelVertexInput input, ref VoxelVertexOutput target)
		{
			target.SubmeshID = IsOpaque ? 0 : 1;

			target.Colour = Colour;
			target.UVs = new Vector4[2 + (CustomUVs != null ? CustomUVs.Length : 0)];

			// Calulcate face UVs (Don't bother clampping)
			Vector3Int absNormal = new Vector3Int(
				Mathf.Abs(input.Normal.x),
				Mathf.Abs(input.Normal.y),
				Mathf.Abs(input.Normal.z)
			);

			Vector3 uvPos = input.Coord + input.CoordOffset;
			Vector2 uvs;

			if (absNormal.x > absNormal.y && absNormal.x > absNormal.z)
				uvs = new Vector2(uvPos.y, uvPos.z);
			else if (absNormal.y > absNormal.x && absNormal.y > absNormal.z)
				uvs = new Vector2(uvPos.x, uvPos.z);
			else
				uvs = new Vector2(uvPos.x, uvPos.y);

			uvs += new Vector2(0.5f, 0.5f);

			target.UVs[0] = Tint;
			target.UVs[1] = new Vector4(Specular, Roughness, uvs.x, uvs.y);

			if (CustomUVs != null)
			{
				int i = 0;
				foreach (Vector4 uv in CustomUVs)
					target.UVs[2 + i++] = uv;
			}
		}

		public static VoxelMaterialProperties Lerp(VoxelMaterialProperties a, VoxelMaterialProperties b, float t)
		{
			VoxelMaterialProperties output = new VoxelMaterialProperties();
			output.Colour = Color.Lerp(a.Colour, b.Colour, t);
			output.Tint = Vector4.Lerp(a.Tint, b.Tint, t);
			output.Specular = Mathf.Lerp(a.Specular, b.Specular, t);
			output.Roughness = Mathf.Lerp(a.Roughness, b.Roughness, t);

			int customUVCount = Mathf.Max(a.CustomUVCount, b.CustomUVCount);
			if (customUVCount != 0)
			{
				output.CustomUVs = new Vector4[customUVCount];

				for (int i = 0; i < customUVCount; ++i)
				{
					Vector4 aUV = i < a.CustomUVCount ? a.CustomUVs[i] : Vector4.zero;
					Vector4 bUV = i < b.CustomUVCount ? b.CustomUVs[i] : Vector4.zero;

					output.CustomUVs[i] = Vector4.Lerp(aUV, bUV, t);
				}
			}

			return output;
		}

		public bool Equals(VoxelMaterialProperties other)
		{
			return Colour == other.Colour &&
				Tint == other.Tint &&
				Specular == other.Specular &&
				Roughness == other.Roughness &&
				Enumerable.SequenceEqual(CustomUVs ?? new Vector4[0], other.CustomUVs ?? new Vector4[0]);
		}

		public override bool Equals(object obj)
		{
			if (obj is VoxelMaterialProperties)
				return Equals((VoxelMaterialProperties)obj);

			return false;
		}

		public override int GetHashCode()
		{
			int hash = 17;
			hash = hash * 31 + Colour.GetHashCode();
			hash = hash * 31 + Tint.GetHashCode();
			hash = hash * 31 + Specular.GetHashCode();
			hash = hash * 31 + Roughness.GetHashCode();

			if(CustomUVCount != 0)
				hash = hash * 31 + CustomUVs.GetHashCode();
			return hash;
		}

		public static bool operator==(VoxelMaterialProperties a, VoxelMaterialProperties b)
		{
			return a.Equals(b);
		}

		public static bool operator!=(VoxelMaterialProperties a, VoxelMaterialProperties b)
		{
			return !a.Equals(b);
		}
	}
}
