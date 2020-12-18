using DQR.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Voxel
{
	public enum VoxelFace
	{
		Top,
		Bottom,
		Left,
		Right,
		Front,
		Back
	}

	[System.Flags]
	public enum VoxelFaces
	{
		None = 0,
		Top = 1,
		Bottom = 2,
		Left = 4,
		Right = 8,
		Front = 16,
		Back = 32,
		All = Top | Bottom | Left | Right | Front | Back
	}

	public static class VoxelFaceHelpers
	{
		public static VoxelFaces ToFaces(this VoxelFace face)
		{
			switch (face)
			{
				case (VoxelFace.Top):
					return VoxelFaces.Top;
				case (VoxelFace.Bottom):
					return VoxelFaces.Bottom;
				case (VoxelFace.Left):
					return VoxelFaces.Left;
				case (VoxelFace.Right):
					return VoxelFaces.Right;
				case (VoxelFace.Front):
					return VoxelFaces.Front;
				case (VoxelFace.Back):
					return VoxelFaces.Back;
				default:
					return VoxelFaces.None;
			}
		}

		public static IEnumerable<VoxelFace> ToFaceCollection(this VoxelFaces faces)
		{
			List<VoxelFace> output = new List<VoxelFace>();
			if (faces.HasFlag(VoxelFaces.Top))
				output.Add(VoxelFace.Top);
			if (faces.HasFlag(VoxelFaces.Bottom))
				output.Add(VoxelFace.Bottom);

			if (faces.HasFlag(VoxelFaces.Left))
				output.Add(VoxelFace.Left);
			if (faces.HasFlag(VoxelFaces.Right))
				output.Add(VoxelFace.Right);

			if (faces.HasFlag(VoxelFaces.Front))
				output.Add(VoxelFace.Front);
			if (faces.HasFlag(VoxelFaces.Back))
				output.Add(VoxelFace.Back);

			return output;
		}

		public static VoxelFace NormalToFace(Vector3 normal)
		{
			Vector3 absNormal = new Vector3(
				Mathf.Abs(normal.x),
				Mathf.Abs(normal.y),
				Mathf.Abs(normal.z)
			);

			if (absNormal.x > absNormal.y && absNormal.x > absNormal.z)
			{
				return normal.x < 0 ? VoxelFace.Left : VoxelFace.Right;
			}
			else if (absNormal.y > absNormal.x && absNormal.y > absNormal.z)
			{
				return normal.y < 0 ? VoxelFace.Bottom : VoxelFace.Top;
			}
			else
			{
				return normal.z < 0 ? VoxelFace.Back : VoxelFace.Front;
			}
		}

		public static VoxelFace NormalToFace(Vector3Int normal)
		{
			Vector3Int absNormal = new Vector3Int(
				Mathf.Abs(normal.x),
				Mathf.Abs(normal.y),
				Mathf.Abs(normal.z)
			);

			if (absNormal.x > absNormal.y && absNormal.x > absNormal.z)
			{
				return normal.x < 0 ? VoxelFace.Left : VoxelFace.Right;
			}
			else if (absNormal.y > absNormal.x && absNormal.y > absNormal.z)
			{
				return normal.y < 0 ? VoxelFace.Bottom : VoxelFace.Top;
			}
			else
			{
				return normal.z < 0 ? VoxelFace.Back : VoxelFace.Front;
			}
		}
	}
}
