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
			
			var splitDefines = new HashSet<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget).Split(';'));

			// Strip any previous DQR defines
			// DQR_MANUAL_... used to maintain DQR specific defines
			// -DQR_MANUAL_OVERRIDE_DEFINES: Disables auto defines below (Allowing anything to be specified in settings)
			HashSet<string> currentDefines = new HashSet<string>(splitDefines.Where((define) => !define.StartsWith("DQR_") || define.StartsWith("DQR_MANUAL_")));
			
			if (!currentDefines.Contains("DQR_MANUAL_OVERRIDE_DEFINES"))
			{
				var req = Client.Search("com.realgames.dear-imgui", true);
				
				if (EditorUserBuildSettings.development)
				{
					currentDefines.Add("DQR_DEV");

					//if (EditorUserBuildSettings.enableHeadlessMode) // Disable in server builds?
					{
						currentDefines.Add("DQR_ASSERTS");
					}
				}

				// Wait here for request to finish
				while (!req.IsCompleted)
				{
					Thread.Yield();
				}

				if (req.Status == StatusCode.Success)
				{
					currentDefines.Add("DQR_FEATURE_DEAR_IMGUI");
				}

				PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, string.Join(";", currentDefines));
			}
		}
#endif

		public static Vector3Int ClosestPoint(this BoundsInt bounds, Vector3Int point)
		{
			return Vector3Int.Max(bounds.min, Vector3Int.Min(bounds.max, point));
		}
	}
}
