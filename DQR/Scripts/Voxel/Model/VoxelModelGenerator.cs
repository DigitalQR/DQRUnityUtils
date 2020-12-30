using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DQR.Tasks;

namespace DQR.Voxel
{	
	public class VoxelModelGenerationRequest
	{	
		private VoxelModelGenerateTask m_Generator;
		private TaskHandle m_Task;

		private Mesh m_OutputMesh;
		private Material[] m_OutputMaterials;

		private VoxelModelGenerationRequest(VoxelModelGenerateTask generator, bool async)
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
			m_OutputMaterials = null;
		}

		public static VoxelModelGenerationRequest NewModelRequestAsync(IVoxelVolume volume, IVoxelMaterialResolver resolver, VoxelModelGenerationSettings settings)
		{
			VoxelModelGenerationRequest request = new VoxelModelGenerationRequest(new VoxelModelGenerateTask(volume, resolver, settings), true);
			return request;
		}

		public static VoxelModelGenerationRequest NewModelRequestSync(IVoxelVolume volume, IVoxelMaterialResolver resolver, VoxelModelGenerationSettings settings)
		{
			VoxelModelGenerationRequest request = new VoxelModelGenerationRequest(new VoxelModelGenerateTask(volume, resolver, settings), false);
			return request;
		}

		public bool HasFinishedProcessing()
		{
			return m_OutputMesh != null || m_Task.IsCompleted;
		}

		public void GatherOutput(out Mesh mesh, out Material[] materials)
		{
			if (m_OutputMesh == null)
			{
				if (m_Task != null)
				{
					m_Task.AwaitCompletion();
				}

				m_OutputMesh = new Mesh();
				m_Generator.UploadData(m_OutputMesh, out m_OutputMaterials);

				// Release everything except the output
				m_Task = null;
				m_Generator = default;
			}

			mesh = m_OutputMesh;
			materials = m_OutputMaterials;
		}
	}
}
