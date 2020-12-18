﻿using System.Collections;
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
	public class DQRBuildSettings : ScriptableObject
	{
		[Header("General")]
		public bool m_EnableAsserts;
		public bool m_AutoGenerateDefinitions;
		public string m_AdditionalDefines;

		[Header("Features")]
		public bool m_BuildImGuiExt;
		public bool m_BuildObjectDB;
		public bool m_BuildVoxel;

		public DQRBuildSettings()
		{
			RestoreDefaults();
		}

		public void RestoreDefaults()
		{
			m_EnableAsserts = true;
			m_AutoGenerateDefinitions = true;
			m_AdditionalDefines = "";

			m_BuildImGuiExt = true;
			m_BuildObjectDB = false;
			m_BuildVoxel = false;
		}

#if UNITY_EDITOR
		public static DQRBuildSettings GetOrCreateSettings()
		{
			if (!AssetDatabase.IsValidFolder("Assets/DQR/Generated"))
			{
				AssetDatabase.CreateFolder("Assets/DQR", "Generated");
			}

			var settings = AssetDatabase.LoadAssetAtPath<DQRBuildSettings>("Assets/DQR/Generated/DQRBuildSettings.asset");
			if (settings == null)
			{
				settings = ScriptableObject.CreateInstance<DQRBuildSettings>();
				settings.RestoreDefaults();

				AssetDatabase.CreateAsset(settings, "Assets/DQR/Generated/DQRBuildSettings.asset");
				AssetDatabase.SaveAssets();
			}

			return settings;
		}
#endif

		public void CommitChanges()
		{
#if UNITY_EDITOR
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh(); // Force rebuild
#endif
		}
	}

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

			DQRBuildSettings settings = DQRBuildSettings.GetOrCreateSettings();

			// Strip any previous DQR defines
			// DQR_MANUAL_... used to maintain DQR specific defines
			// -DQR_MANUAL_OVERRIDE_DEFINES: Disables auto defines below (Allowing anything to be specified in settings)
			HashSet<string> currentDefines = new HashSet<string>(splitDefines.Where((define) => !define.StartsWith("DQR_")));
			
			if (settings.m_AutoGenerateDefinitions)
			{
				// Feature defines
				currentDefines.Add("DQR_FEATURE_UTILS");

				if (settings.m_BuildImGuiExt && currentDefines.Contains("IMGUI_FEATURE_FREETYPE"))
				{
					currentDefines.Add("DQR_FEATURE_DEAR_IMGUI");
				}

				if (settings.m_BuildVoxel)
				{
					currentDefines.Add("DQR_FEATURE_VOXEL");
				}

				if (settings.m_BuildObjectDB)
				{
					currentDefines.Add("DQR_FEATURE_OBJECTDB");
				}

				// Misc defines
				if (settings.m_EnableAsserts)
				{
					currentDefines.Add("DQR_ASSERTS");
				}

				foreach (var define in settings.m_AdditionalDefines.Split(';'))
				{
					currentDefines.Add(define);
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
