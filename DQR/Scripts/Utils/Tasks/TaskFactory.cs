using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;
using System.Diagnostics;

namespace DQR.Tasks
{
	/// <summary>
	/// A basic wrapper for dispatching tasks through System.Tasks (TODO - Re look at this later)
	/// </summary>
	public class TaskFactory
	{
		private static TaskFactory s_Instance = null;
		public static TaskFactory Instance
		{
			get
			{
				if (s_Instance == null)
					s_Instance = new TaskFactory();

				return s_Instance;
			}
		}

		private TaskFactory()
		{
		}

		public TaskHandle StartNew(ITask input)
		{
			Task task = Task.Factory.StartNew(() =>
			{
#if DQR_DEV
				if (Debugger.IsAttached)
				{
					input.ExecuteTask();
				}
				else
#endif
				{

					try
					{
						input.ExecuteTask();
					}
					catch (System.Exception e)
					{
						Assert.FailFormat("Exception caught in task '{0}': '{1}'\n\n{2}", input.ToString(), e.Message, e.StackTrace);
					}
				}
			});
			return ProcessNewHandle(new TaskHandle_Task(task));
		}

		private TaskHandle ProcessNewHandle(TaskHandle handle)
		{
			return handle;
		}
	}
}
