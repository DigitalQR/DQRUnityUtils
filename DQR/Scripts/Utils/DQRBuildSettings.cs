using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using System.IO;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
#endif

namespace DQR
{
	public class DQRBuildSettings : ScriptableObject
	{
		public const string c_AssetPath = "Assets/DQR/Generated/DQRBuildSettings.asset";

		public bool m_EnableAsserts;
		public string m_AdditionalDefines;
		
		public bool m_BuildObjectDB;
		public bool m_BuildVoxel;
		public bool m_BuildEZInspect;
		
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
			var settings = AssetDatabase.LoadAssetAtPath<DQRBuildSettings>(c_AssetPath);
			if (settings == null)
			{
				settings = ScriptableObject.CreateInstance<DQRBuildSettings>();
				settings.RestoreDefaults();

				string curDir = "";
				foreach (string dir in Path.GetDirectoryName(c_AssetPath).Replace('\\', '/').Split('/'))
				{
					string prevFolder = curDir;
					curDir = Path.Combine(curDir, dir);

					if (!AssetDatabase.IsValidFolder(curDir))
						AssetDatabase.CreateFolder(prevFolder, dir);
				}
				
				AssetDatabase.CreateAsset(settings, c_AssetPath);
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

		internal static SerializedObject GetSerializedSettings()
		{
			return new SerializedObject(GetOrCreateSettings());
		}
	}

	internal class DQRBuildSettingsProvider : SettingsProvider
	{
		private SerializedObject m_SerializedSettings;
		
		public DQRBuildSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
			: base(path, scope)
		{
		}

		public static bool IsSettingsAvailable()
		{
			return File.Exists(DQRBuildSettings.c_AssetPath);
		}

		public override void OnActivate(string searchContext, VisualElement rootElement)
		{
			m_SerializedSettings = DQRBuildSettings.GetSerializedSettings();
		}

		public override void OnGUI(string searchContext)
		{
			m_SerializedSettings.Update();
			EditorGUILayout.LabelField(new GUIContent("General"));
			EditorGUILayout.PropertyField(m_SerializedSettings.FindProperty("m_EnableAsserts"), new GUIContent("Enable Asserts"));
			EditorGUILayout.PropertyField(m_SerializedSettings.FindProperty("m_AdditionalDefines"), new GUIContent("AdditionalDefines"));

			EditorGUILayout.LabelField(new GUIContent("Features"));
			EditorGUILayout.PropertyField(m_SerializedSettings.FindProperty("m_BuildObjectDB"), new GUIContent("ObjectDB"));
			EditorGUILayout.PropertyField(m_SerializedSettings.FindProperty("m_BuildVoxel"), new GUIContent("Voxel"));
			EditorGUILayout.PropertyField(m_SerializedSettings.FindProperty("m_BuildEZInspect"), new GUIContent("EZInspect", "DearImGUI based inspector (Requires Unity ImGUI version)"));

			m_SerializedSettings.ApplyModifiedProperties();

			if (GUILayout.Button("Refresh Project"))
			{
				AssetDatabase.Refresh();
			}
		}

		// Register the SettingsProvider
		[SettingsProvider]
		public static SettingsProvider CreateSmartNSSettingsProvider()
		{
			//DQRBuildSettings.GetOrCreateSettings();

			if (IsSettingsAvailable())
			{
				var provider = new DQRBuildSettingsProvider("Project/DQRUtils", SettingsScope.Project);
				provider.label = "DigitalQR Utilities";
				provider.keywords = new HashSet<string>(new[] { "DigitalQR", "DQR", "Utils" });
				return provider;
			}
			
			return null;
		}
	}
}