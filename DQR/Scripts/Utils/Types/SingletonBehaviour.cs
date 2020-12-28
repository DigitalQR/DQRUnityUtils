using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Types
{
	public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
	{
		private static T s_Instance;
		public static T Instance
		{
			get
			{
				Assert.Format(IsValid, "Singleton '{0}' is invalid", typeof(T).Name);
				return s_Instance;
			}
		}

		public static bool IsValid
		{
			get => s_Instance;
		}

		private void Awake()
		{
			// Toggle on/off so ignore this awake call
			if (s_Instance == this)
				return;

			if (!IsValid)
			{
				s_Instance = this as T;
				Log.Info<DefaultLogCategory>("Singleton '{0}:{1}' registered", typeof(T).Name, gameObject.name);
				SingletonInit();
			}
			else
			{
				Assert.FailFormat("Singleton '{0}' has already been registered", typeof(T).Name);
				Destroy(gameObject);
			}
		}

		private void OnDestroy()
		{
			if (s_Instance == this)
			{
				s_Instance = null;
				Log.Info<DefaultLogCategory>("Singleton '{0}:{1}' destroyed", typeof(T).Name, gameObject.name);
				SingletonDestruct();
			}
		}

		protected virtual void SingletonInit()
		{
		}

		protected virtual void SingletonDestruct()
		{
		}
	}
}