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
	public class DQRBuildSettings : ScriptableObject
	{
		[Header("General")]
		public bool m_EnableAsserts;
		public string m_AdditionalDefines;

		[Header("Features")]
		public bool m_BuildObjectDB;
		public bool m_BuildVoxel;
		public bool m_BuildEZInspect;

		public DQRBuildSettings()
		{
			RestoreDefaults();
		}

		public void RestoreDefaults()
		{
			m_EnableAsserts = true;
			m_AdditionalDefines = "";

			m_BuildObjectDB = false;
			m_BuildVoxel = false;
			m_BuildEZInspect = false;
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
}
