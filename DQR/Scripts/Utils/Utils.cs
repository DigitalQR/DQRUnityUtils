using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
#endif

namespace DQR
{
	public static class DQRUtils
	{
		// This relies on Unity's main thread being used for initialization (May not work, so will just have to see)
		private static Thread s_MainThread = Thread.CurrentThread;

		public static bool InMainThread
		{
			get => Thread.CurrentThread == s_MainThread;
		}

#if UNITY_EDITOR
		[InitializeOnLoadMethod]
		static void SetupLibrary()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
				return;
			
			//BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
			BuildTargetGroup buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
			List<string> currentDefines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget).Split(';'));
			
			if (EditorUserBuildSettings.development)
			{
				currentDefines.Add("DQR_DEV");

				//if (EditorUserBuildSettings.enableHeadlessMode) // Disable in server builds?
				{
					currentDefines.Add("DQR_ASSERTS");
				}
			}

			PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, string.Join(";", currentDefines));
		}
#endif

		public static Vector3Int ClosestPoint(this BoundsInt bounds, Vector3Int point)
		{
			return Vector3Int.Max(bounds.min, Vector3Int.Min(bounds.max, point));
		}
	}
}
