using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DQR.Debug;
using DQR.Tasks;

namespace DQR.Voxel
{
	[System.Serializable]
	public class VoxelModelGenerationSettings
	{
		public Vector3 Scale = Vector3.one;
		public Vector3 NormalizedPivot = new Vector3(0.5f, 0.5f, 0.5f);
	}
	
	public class VoxelModelGenerationRequest
	{
		public struct GenerationJob : ITask
		{
			private class IntermediateData
			{
				//public Material m_Material;

				public List<Vector3> Positions = new List<Vector3>();
				public List<Vector3> Normals = new List<Vector3>();
				public List<List<Vector4>> UVs = new List<List<Vector4>>();
				public List<Color32> Colours = new List<Color32>();

				public Dictionary<int, List<int>> SubmeshTriangleIndices = new Dictionary<int, List<int>>();
			}
			
			private IVoxelVolume m_Volume;
			private IVoxelMaterialResolver m_MaterialResolver;
			private VoxelModelGenerationSettings m_Settings;
			private IntermediateData m_Intermediate;
							
			public GenerationJob(IVoxelVolume volume, IVoxelMaterialResolver resolver, VoxelModelGenerationSettings settings)
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
							Vector3Int coord = new Vector3Int(x,y,z);
							if (m_Volume.TryGetVoxelCell(coord, out VoxelCell cell) && m_MaterialResolver.ShouldConsiderForModel(coord, cell))
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

			public Mesh UploadMesh()
			{
				Mesh mesh = new Mesh();

				mesh.SetVertices(m_Intermediate.Positions);

				if (m_Intermediate.Normals.Count != 0)
					mesh.SetNormals(m_Intermediate.Normals);

				mesh.SetColors(m_Intermediate.Colours);

				for (int c = 0; c < m_Intermediate.UVs.Count; ++c)
					mesh.SetUVs(c, m_Intermediate.UVs[c].ToArray());

				foreach (var submesh in m_Intermediate.SubmeshTriangleIndices)
					mesh.SetIndices(submesh.Value.ToArray(), MeshTopology.Triangles, submesh.Key);

				if (m_Intermediate.Normals.Count == 0)
					mesh.RecalculateNormals();

				return mesh;
			}

			private void ConsiderAddingFace(Vector3Int coord, VoxelCell cell, Vector3Int normal)
			{
				Vector3Int targetCoord = coord + normal;
				VoxelCell targetCell = VoxelCell.Invalid;
				m_Volume.TryGetVoxelCell(targetCoord, out targetCell);

				if (m_MaterialResolver.ShouldAddFace(coord, cell, targetCoord, targetCell))
					AddFace(coord, normal, cell);
			}

			private void AddFace(Vector3Int coord, Vector3Int normal, VoxelCell cell)
			{
				int sign = 0;
				int i0 = -1;
				int i1 = -1;
				int i2 = -1;
				int i3 = -1;
				int m0 = -1;
				int m1 = -1;
				int m2 = -1;
				int m3 = -1;

				// Add left/right
				if (normal.x != 0)
				{
					sign = normal.x >= 0 ? 1 : -1;
					
					i0 = AddVertex(coord, normal, cell, new Vector3(1 * sign, 1, 1) * 0.5f, out m0);
					i1 = AddVertex(coord, normal, cell, new Vector3(1 * sign, 1, -1) * 0.5f, out m1);
					i2 = AddVertex(coord, normal, cell, new Vector3(1 * sign, -1, 1) * 0.5f, out m2);
					i3 = AddVertex(coord, normal, cell, new Vector3(1 * sign, -1, -1) * 0.5f, out m3);
				}

				// Add top/bottom
				if (normal.y != 0)
				{
					sign = normal.y >= 0 ? 1 : -1;
					
					i0 = AddVertex(coord, normal, cell, new Vector3(1, 1 * sign, 1) * 0.5f, out m0);
					i1 = AddVertex(coord, normal, cell, new Vector3(-1, 1 * sign, 1) * 0.5f, out m1);
					i2 = AddVertex(coord, normal, cell, new Vector3(1, 1 * sign, -1) * 0.5f, out m2);
					i3 = AddVertex(coord, normal, cell, new Vector3(-1, 1 * sign, -1) * 0.5f, out m3);
				}

				// Add front/back
				else if (normal.z != 0)
				{
					sign = normal.z >= 0 ? 1 : -1;
					
					i0 = AddVertex(coord, normal, cell, new Vector3(-1, 1, 1 * sign) * 0.5f, out m0);
					i1 = AddVertex(coord, normal, cell, new Vector3(1, 1, 1 * sign) * 0.5f, out m1);
					i2 = AddVertex(coord, normal, cell, new Vector3(-1, -1, 1 * sign) * 0.5f, out m2);
					i3 = AddVertex(coord, normal, cell, new Vector3(1, -1, 1 * sign) * 0.5f, out m3);
				}

				Assert.Format(m0 == m1 && m0 == m2 && m0 == m3, "SubmeshID doesn't match for each face ({0}, {1}, {2}, {3})", m0, m1, m2, m3);
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
			
			private int AddVertex(Vector3Int coord, Vector3Int normal, VoxelCell cell, Vector3 offset, out int submeshID)
			{
				VoxelVertexInput input = new VoxelVertexInput
				{
					SourceVolume = m_Volume,
					Coord = coord,
					CoordOffset = offset,
					Normal = normal,
					Cell = cell
				};

				return AddVertex(input, out submeshID);
			}

			private int AddVertex(VoxelVertexInput input, out int submeshID)
			{
				VoxelVertexOutput vertex = m_MaterialResolver.ResolveVoxelVertex(input, m_Settings);
				int vertexIndex = m_Intermediate.Positions.Count;

				m_Intermediate.Positions.Add(vertex.Position);
				m_Intermediate.Normals.Add(vertex.Normal);
				m_Intermediate.Colours.Add(vertex.Colour);

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

				submeshID = vertex.SubmeshID;
				return vertexIndex;
			}
		}
		
		private GenerationJob m_Generator;
		private TaskHandle m_Task;
		private Mesh m_OutputMesh;

		private VoxelModelGenerationRequest(GenerationJob generator, bool async)
		{
			m_Generator = generator;
			if (async)
			{
				m_Task = TaskFactory.Instance.StartNew(generator);
			}
			else
			{
				m_Task = null;
				generator.ExecuteTask();
			}
			m_OutputMesh = null;
		}

		public static VoxelModelGenerationRequest NewModelRequestAsync(IVoxelVolume volume, IVoxelMaterialResolver resolver, VoxelModelGenerationSettings settings)
		{
			VoxelModelGenerationRequest request = new VoxelModelGenerationRequest(new GenerationJob(volume, resolver, settings), true);
			return request;
		}

		public static VoxelModelGenerationRequest NewModelRequestSync(IVoxelVolume volume, IVoxelMaterialResolver resolver, VoxelModelGenerationSettings settings)
		{
			VoxelModelGenerationRequest request = new VoxelModelGenerationRequest(new GenerationJob(volume, resolver, settings), false);
			return request;
		}

		public bool HasFinishedProcessing()
		{
			return m_OutputMesh != null || m_Task.IsCompleted;
		}

		public Mesh GetMeshOutput()
		{
			if (m_OutputMesh == null)
			{
				if (m_Task != null)
				{
					m_Task.AwaitCompletion();
				}

				m_OutputMesh = m_Generator.UploadMesh();

				// Release everything except the output
				m_Task = null;
				m_Generator = default;
			}

			return m_OutputMesh;
		}
	}
}
