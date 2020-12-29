using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ImGuiNET;
using ImGuiNET.Unity;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

using DQR.Types;

namespace DQR.EZInspect
{
	public class EZInspectLogCategory : LogCategory{ } 

	public class EZInspector : SingletonBehaviour<EZInspector>, IEZInspectionListener
	{
		[SerializeField]
		private float m_BaseResolutionWidth = 1920;

		private bool m_HideAll = false;

		private bool m_ResolutionJustChanged = false;
		private bool m_WindowSettingsDirty = false;

		private PropertyPrefVector2Int m_LastKnownResolution = new PropertyPrefVector2Int("DQR.EZInspect.LastKnownRes", Vector2Int.zero);
		private PropertyPrefFloat m_FontScale = new PropertyPrefFloat("DQR.EZInspect.FontScale", 1.0f);
		private PropertyPrefBool m_ResetOnResolutionScale = new PropertyPrefBool("DQR.EZInspect.ResetOnResolution", true);
		private PropertyPrefBool m_ApplyResolutionScale = new PropertyPrefBool("DQR.EZInspect.ApplyResolutionScale", true);

		private Dictionary<IEZInspectionListener, EZInspectionListenerContainer> m_InspectListeners = new Dictionary<IEZInspectionListener, EZInspectionListenerContainer>();
		
		protected override void SingletonInit()
		{
			SetupListenerContainerIfRequired(gameObject);
			ImGuiUn.Layout += OnImGuiLayout;

#if UNITY_EDITOR
			EditorApplication.hierarchyChanged += OnEditorHierarchyChanged;
#endif
		}

#if ENABLE_LEGACY_INPUT_MANAGER
		public void Update()
		{
			// Toggle with '`'
			if (Input.GetKeyDown(KeyCode.BackQuote))
			{
				OnOpenEZInspector();
			}
		}
#endif
		
		public float RenderResolutionScale
		{
			get => (m_ApplyResolutionScale ? (float)Screen.width / m_BaseResolutionWidth : 1.0f);
		}
				
		public bool IsInspectorActive
		{
			get
			{
				if(m_InspectListeners.TryGetValue(this, out EZInspectionListenerContainer container))
					return container.IsActive;
				return false;
			}
			set
			{
				if(m_InspectListeners.TryGetValue(this, out EZInspectionListenerContainer container))
					container.IsActive = value;
			}
		}

		private void OnImGuiLayout()
		{
			// Use hidden window to detect when resolution just changed
			{
				bool justChanged = m_ResolutionJustChanged;
				m_ResolutionJustChanged = false;

				Vector2Int currentResolution = new Vector2Int(Screen.width, Screen.height);

				if (m_WindowSettingsDirty || currentResolution != m_LastKnownResolution)
				{
					Log.Info<EZInspectLogCategory>("Resolution change detected");
					m_ResolutionJustChanged = true;
					m_WindowSettingsDirty = false;
					m_LastKnownResolution.Set(currentResolution);
				}
			}
			
			// Render overlays
			ImGui.GetIO().FontGlobalScale = m_FontScale;
			ImGui.GetIO().ConfigDockingTransparentPayload = true;
			ImGui.DockSpaceOverViewport(ImGui.GetMainViewport() ,ImGuiDockNodeFlags.PassthruCentralNode);

			foreach (var kvp in m_InspectListeners)
			{
				var inspect = kvp.Value;
				if ((!m_HideAll || kvp.Key.Equals(this)) && inspect.IsActive)
				{
					inspect.OnInspect(this);
				}
			}
		}

		public void RegisterListener(GameObject context, IEZInspectionListener listener)
		{
			Assert.Format(!m_InspectListeners.ContainsKey(listener), "Already registered listener '{0}.{1}'", context.name, listener);
			m_InspectListeners.Add(listener, new EZInspectionListenerContainer(context, listener));
		}

		public bool UnregisterListener(IEZInspectionListener listener)
		{
			return m_InspectListeners.Remove(listener);
		}

#if UNITY_EDITOR
		private void OnEditorHierarchyChanged()
		{
			if (EditorApplication.isPlaying)
			{
				foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
				{
					SetupListenerContainerIfRequired(obj);
				}
			}
		}
#endif
		private void SetupListenerContainerIfRequired(GameObject obj)
		{
			bool hasInspector = obj.GetComponent<IEZInspectionListener>() != null;

			if (hasInspector)
			{
				bool hasContainer = obj.GetComponent<EZInspectionContainer>() != null;
				if (!hasContainer)
				{
					obj.AddComponent<EZInspectionContainer>();
				}
			}
		}
		
		public bool BeginWindow(string name, Vector2 initPos, Vector2 initSize)
		{
			ImGuiCond propCondition = m_ResolutionJustChanged ? ImGuiCond.Always : ImGuiCond.FirstUseEver;
			
			ImGui.SetNextWindowPos(initPos * RenderResolutionScale, propCondition);
			ImGui.SetNextWindowSize(initSize * RenderResolutionScale, propCondition);

			bool open = true;
			return ImGui.Begin(name, ref open, ImGuiWindowFlags.NoCollapse) && open;
		}

		public void EndWindow()
		{
			ImGui.End();
		}

		private void OnOpenEZInspector()
		{
			IsInspectorActive = !IsInspectorActive;
		}

#region Inspector Overlay
		private string m_SearchFilter = "";

		public InspectionProperties GetInspectProperties()
		{
			InspectionProperties props = new InspectionProperties();
			props.TitleOverride = "EZInspector";
			return props;
		}

		private bool ContainedInSearchFilter(string name)
		{
			if (string.IsNullOrWhiteSpace(m_SearchFilter))
				return true;

			return m_SearchFilter.Split(' ').Where((filter) => !string.IsNullOrWhiteSpace(filter) && name.IndexOf(filter, System.StringComparison.CurrentCultureIgnoreCase) >= 0).Any();
		}

		public void OnInspect(EZInspector inspector)
		{
			if (inspector != this)
				return;

			ImGui.Checkbox("Hide All", ref m_HideAll);

			ImGui.BeginTabBar("##inspector_tabs");
			if (ImGui.BeginTabItem("Inspectors"))
			{
				ImGui.InputText("Filter", ref m_SearchFilter, 1024);
				ImGui.Text("Registered " + (m_InspectListeners.Count - 1) + " inspectors");
				ImGui.Spacing();

				ImGui.BeginChild("scroll_area");

				foreach (var kvp in m_InspectListeners)
				{
					if (kvp.Key.Equals(this))
						continue;

					var inspect = kvp.Value;
					string inspectName = inspect.FullTitle;

					if (ContainedInSearchFilter(inspectName))
					{
						bool visible = inspect.IsActive;
						if (ImGui.Checkbox(inspectName, ref visible))
						{
							inspect.IsActive = visible;
						}
					}
				}

				ImGui.EndChild();
				ImGui.EndTabItem();
			}

			if (ImGui.BeginTabItem("Settings"))
			{
				float fontScale = m_FontScale;
				bool resetOnResolutionScale = m_ResetOnResolutionScale;

				ImGui.SliderFloat("Font Scale", ref fontScale, 0.01f, 10.0f);
				ImGui.Checkbox("Reset on Resolution Change", ref resetOnResolutionScale);

				if (ImGui.Button("Reset Windows"))
				{
					m_WindowSettingsDirty = true;

					foreach (var kvp in m_InspectListeners)
					{
						if(!kvp.Key.Equals(this))
							kvp.Value.IsActive = false;
					}
				}
				ImGui.SameLine();

				if (ImGui.Button("Reset Prefs"))
				{
					fontScale = 1.0f;
					resetOnResolutionScale = true;
				}

				m_FontScale.Set(fontScale);
				m_ResetOnResolutionScale.Set(resetOnResolutionScale);

				ImGui.EndTabItem();
			}

			ImGui.EndTabBar();
		}
#endregion
	}
}
