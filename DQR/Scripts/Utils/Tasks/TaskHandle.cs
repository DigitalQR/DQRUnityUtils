using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;

namespace DQR.Tasks
{
	public interface ITask
	{
		void ExecuteTask();
	}

	public abstract class TaskHandle
	{
		public abstract bool IsCompleted { get; }
		public abstract void AwaitCompletion();
	}

	internal class TaskHandle_Task : TaskHandle
	{
		private Task m_InternalHandle;

		public TaskHandle_Task(Task handle)
		{
			m_InternalHandle = handle;
		}

		public override bool IsCompleted
		{
			get => m_InternalHandle.IsCompleted;
		}

		public override void AwaitCompletion()
		{
			m_InternalHandle.Wait();
		}
	}

	internal abstract class TaskHandle_Job : TaskHandle
	{
		private JobHandle m_InternalHandle;

		public TaskHandle_Job(JobHandle handle)
		{
			m_InternalHandle = handle;
		}

		public override bool IsCompleted
		{
			get => m_InternalHandle.IsCompleted;
		}

		public override void AwaitCompletion()
		{
			m_InternalHandle.Complete();
		}
	}
}
