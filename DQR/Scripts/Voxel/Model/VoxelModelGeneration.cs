using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DQR.Tasks;

namespace DQR.Voxel
{
	[System.Serializable]
	public class VoxelModelGenerationSettings
	{
		public Vector3 Scale = Vector3.one;
		public Vector3 NormalizedPivot = new Vector3(0.5f, 0.5f, 0.5f);
	}

	public class VoxelModelGenerateTask : ITask
	{
		private class IntermediateData
		{
			public List<Vector3> Positions = new List<Vector3>();
			public List<Vector3> Normals = new List<Vector3>();
			public List<List<Vector4>> UVs = new List<List<Vector4>>();
			public List<Color32> Colours = new List<Color32>();

			public Dictionary<Material, List<int>> SubmeshTriangleIndices = new Dictionary<Material, List<int>>();
		}

		private IVoxelVolume m_Volume;
		private IVoxelMaterialResolver m_MaterialResolver;
		private VoxelModelGenerationSettings m_Settings;
		private IntermediateData m_Intermediate;

		public VoxelModelGenerateTask(IVoxelVolume volume, IVoxelMaterialResolver resolver, VoxelModelGenerationSettings settings)
		{
			m_Volume = volume;
			m_MaterialResolver = resolver;
			m_Settings = settings;
			m_Intermediate = new IntermediateData();
		}

		public void ExecuteTask()
		{
			BoundsInt bounds = m_Volume.GetVolumeBounds();

			for (int z = bounds.min.z; z <= bounds.max.z; ++z)
				for (int y = bounds.min.y; y <= bounds.max.y; ++y)
					for (int x = bounds.min.x; x <= bounds.max.x; ++x)
					{
						Vector3Int coord = new Vector3Int(x, y, z);
						if (m_Volume.TryGetVoxelCell(coord, out VoxelCell cell))
						{
							ConsiderAddingFace(coord, cell, new Vector3Int(-1, 0, 0));
							ConsiderAddingFace(coord, cell, new Vector3Int(1, 0, 0));
							ConsiderAddingFace(coord, cell, new Vector3Int(0, -1, 0));
							ConsiderAddingFace(coord, cell, new Vector3Int(0, 1, 0));
							ConsiderAddingFace(coord, cell, new Vector3Int(0, 0, -1));
							ConsiderAddingFace(coord, cell, new Vector3Int(0, 0, 1));
						}
					}
		}

		public void UploadData(Mesh mesh, out Material[] materials)
		{
			mesh.SetVertices(m_Intermediate.Positions);

			if (m_Intermediate.Normals.Count != 0)
				mesh.SetNormals(m_Intermediate.Normals);

			mesh.SetColors(m_Intermediate.Colours);

			for (int c = 0; c < m_Intermediate.UVs.Count; ++c)
				mesh.SetUVs(c, m_Intermediate.UVs[c].ToArray());

			List<Material> mats = new List<Material>();
			foreach (var submesh in m_Intermediate.SubmeshTriangleIndices)
			{
				mesh.SetIndices(submesh.Value.ToArray(), MeshTopology.Triangles, mats.Count);
				mats.Add(submesh.Key);
			}
			materials = mats.ToArray();

			if (m_Intermediate.Normals.Count == 0)
				mesh.RecalculateNormals();
		}

		private void ConsiderAddingFace(Vector3Int coord, VoxelCell cell, Vector3Int normal)
		{
			Vector3Int targetCoord = coord + normal;
			VoxelCell targetCell = VoxelCell.Invalid;
			m_Volume.TryGetVoxelCell(targetCoord, out targetCell);

			if (m_MaterialResolver.ShouldAddFace(coord, cell, targetCoord, targetCell, out object scratch))
				AddFace(coord, normal, cell, scratch);
		}

		private void AddFace(Vector3Int coord, Vector3Int normal, VoxelCell cell, object scratch)
		{
			int sign = 0;
			int i0 = -1;
			int i1 = -1;
			int i2 = -1;
			int i3 = -1;
			Material m0 = null;
			Material m1 = null;
			Material m2 = null;
			Material m3 = null;

			// Add left/right
			if (normal.x != 0)
			{
				sign = normal.x >= 0 ? 1 : -1;

				i0 = AddVertex(coord, normal, cell, new Vector3(1 * sign, 1, 1) * 0.5f, scratch, out m0);
				i1 = AddVertex(coord, normal, cell, new Vector3(1 * sign, 1, -1) * 0.5f, scratch, out m1);
				i2 = AddVertex(coord, normal, cell, new Vector3(1 * sign, -1, 1) * 0.5f, scratch, out m2);
				i3 = AddVertex(coord, normal, cell, new Vector3(1 * sign, -1, -1) * 0.5f, scratch, out m3);
			}

			// Add top/bottom
			if (normal.y != 0)
			{
				sign = normal.y >= 0 ? 1 : -1;

				i0 = AddVertex(coord, normal, cell, new Vector3(1, 1 * sign, 1) * 0.5f, scratch, out m0);
				i1 = AddVertex(coord, normal, cell, new Vector3(-1, 1 * sign, 1) * 0.5f, scratch, out m1);
				i2 = AddVertex(coord, normal, cell, new Vector3(1, 1 * sign, -1) * 0.5f, scratch, out m2);
				i3 = AddVertex(coord, normal, cell, new Vector3(-1, 1 * sign, -1) * 0.5f, scratch, out m3);
			}

			// Add front/back
			else if (normal.z != 0)
			{
				sign = normal.z >= 0 ? 1 : -1;

				i0 = AddVertex(coord, normal, cell, new Vector3(-1, 1, 1 * sign) * 0.5f, scratch, out m0);
				i1 = AddVertex(coord, normal, cell, new Vector3(1, 1, 1 * sign) * 0.5f, scratch, out m1);
				i2 = AddVertex(coord, normal, cell, new Vector3(-1, -1, 1 * sign) * 0.5f, scratch, out m2);
				i3 = AddVertex(coord, normal, cell, new Vector3(1, -1, 1 * sign) * 0.5f, scratch, out m3);
			}

			Assert.Format(m0 == m1 && m0 == m2 && m0 == m3, "Material doesn't match for each face ({0}, {1}, {2}, {3})", m0, m1, m2, m3);
			List<int> triangleIndices;

			if (!m_Intermediate.SubmeshTriangleIndices.TryGetValue(m0, out triangleIndices))
			{
				triangleIndices = new List<int>();
				m_Intermediate.SubmeshTriangleIndices.Add(m0, triangleIndices);
			}

			if (sign == 1)
			{
				triangleIndices.Add(i0);
				triangleIndices.Add(i2);
				triangleIndices.Add(i1);

				triangleIndices.Add(i2);
				triangleIndices.Add(i3);
				triangleIndices.Add(i1);
			}
			else
			{
				triangleIndices.Add(i0);
				triangleIndices.Add(i1);
				triangleIndices.Add(i2);

				triangleIndices.Add(i2);
				triangleIndices.Add(i1);
				triangleIndices.Add(i3);
			}
		}

		private int AddVertex(Vector3Int coord, Vector3Int normal, VoxelCell cell, Vector3 offset, object scratch, out Material assignedMaterial)
		{
			VoxelVertexInput input = new VoxelVertexInput
			{
				SourceVolume = m_Volume,
				Coord = coord,
				CoordOffset = offset,
				Normal = normal,
				Cell = cell
			};

			return AddVertex(input, scratch, out assignedMaterial);
		}

		private int AddVertex(VoxelVertexInput input, object scratch, out Material assignedMaterial)
		{
			VoxelVertexOutput vertex = m_MaterialResolver.ResolveVoxelVertex(input, scratch, m_Settings);
			int vertexIndex = m_Intermediate.Positions.Count;

			m_Intermediate.Positions.Add(vertex.Position);
			m_Intermediate.Normals.Add(vertex.Normal);

			// Make sure all UV channels are kept inline
			Vector4[] vertexUVs = vertex.UVs ?? new Vector4[0];

			int channels = Mathf.Max(vertexUVs.Length, m_Intermediate.UVs.Count);
			for (int c = 0; c < channels; ++c)
			{
				Vector4 uv = c < vertexUVs.Length ? vertexUVs[c] : Vector4.zero;

				if (m_Intermediate.UVs.Count <= c)
				{
					List<Vector4> newChannel = new List<Vector4>();

					// Fill channel with default
					for (int i = 0; i < vertexIndex; ++i)
						newChannel.Add(Vector4.zero);

					m_Intermediate.UVs.Add(newChannel);
				}

				m_Intermediate.UVs[c].Add(uv);
			}

			assignedMaterial = vertex.RenderedMaterial;
			return vertexIndex;
		}
	}
}
