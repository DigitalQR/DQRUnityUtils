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
	public class EZInspector : SingletonBehaviour<EZInspector>, IEZInspectionListener
	{
		[SerializeField]
		private float m_BaseResolutionWidth = 1920;

		private bool m_ResolutionJustChanged = false;
		private bool m_WindowSettingsDirty = false;

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
				IsOverlayActive = !IsOverlayActive;
			}
		}
#endif

		private Vector2Int LastKnownResolution
		{
			get => new Vector2Int(PlayerPrefs.GetInt("EZInspect.LastKnownRes.x", 0), PlayerPrefs.GetInt("EZInspect.LastKnownRes.y", 0));
			set
			{
				PlayerPrefs.SetInt("EZInspect.LastKnownRes.x", value.x);
				PlayerPrefs.SetInt("EZInspect.LastKnownRes.y", value.y);
			}
		}

		private float FontScale
		{
			get => PlayerPrefs.GetFloat("EZInspect.FontScale", 1.0f);
			set => PlayerPrefs.SetFloat("EZInspect.FontScale", value);
		}

		private bool ResetOnResolutionScale
		{
			get => PlayerPrefs.GetInt("EZInspect.ResetOnResolution", 1) == 1;
			set => PlayerPrefs.SetInt("EZInspect.ResetOnResolution", value ? 1 : 0);
		}

		private bool ApplyResolutionScale
		{
			get => PlayerPrefs.GetInt("EZInspect.ApplyResolutionScale", 1) == 1;
			set => PlayerPrefs.SetInt("EZInspect.ApplyResolutionScale", value ? 1 : 0);
		}

		public float RenderResolutionScale
		{
			get => (ApplyResolutionScale ? (float)Screen.width / m_BaseResolutionWidth : 1.0f);
		}
				
		public bool IsOverlayActive
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

				if (m_WindowSettingsDirty || currentResolution != LastKnownResolution)
				{
					Debug.Log("DebugMenu: Resolution change detected");
					m_ResolutionJustChanged = true;
					m_WindowSettingsDirty = false;
					LastKnownResolution = currentResolution;
				}
			}
			
			// Render overlays
			ImGui.GetIO().FontGlobalScale = FontScale;
			ImGui.GetIO().ConfigDockingTransparentPayload = true;
			ImGui.DockSpaceOverViewport(ImGui.GetMainViewport() ,ImGuiDockNodeFlags.PassthruCentralNode);

			foreach (var inspect in m_InspectListeners.Values)
			{
				if (inspect.IsActive)
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
		
		public bool BeginWindow(string name, Vector2 initPos, Vector2 initSize, ref bool open)
		{
			ImGuiCond propCondition = m_ResolutionJustChanged ? ImGuiCond.Always : ImGuiCond.FirstUseEver;

			ImGui.SetNextWindowCollapsed(false, ImGuiCond.Always);
			ImGui.SetNextWindowPos(initPos * RenderResolutionScale, propCondition);
			ImGui.SetNextWindowSize(initSize * RenderResolutionScale, propCondition);
			
			return ImGui.Begin(name, ref open);
		}

		public void EndWindow()
		{
			ImGui.End();
		}

		private void OnOpenEZInspector()
		{
			IsOverlayActive = !IsOverlayActive;
		}

#region Inspector Overlay
		private string m_SearchFilter = "";

		public InspectionProperties GetInspectProperties()
		{
			InspectionProperties props = new InspectionProperties();
			props.ManuallyHandleAllRendering = true;
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

			bool open = true;
			if (BeginWindow("EZInspector", new Vector2(5, 770), new Vector2(330, 300), ref open))
			{
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
					float fontScale = FontScale;
					bool resetOnResolutionScale = ResetOnResolutionScale;

					ImGui.SliderFloat("Font Scale", ref fontScale, 0.01f, 10.0f);
					ImGui.Checkbox("Reset on Resolution Change", ref resetOnResolutionScale);

					if (ImGui.Button("Reset Windows"))
					{
						m_WindowSettingsDirty = true;
					}
					ImGui.SameLine();

					if (ImGui.Button("Reset Prefs"))
					{
						fontScale = 1.0f;
						resetOnResolutionScale = true;
					}

					FontScale = fontScale;
					ResetOnResolutionScale = resetOnResolutionScale;

					ImGui.EndTabItem();
				}

				ImGui.EndTabBar();
				EndWindow();
			}

			if (!open)
				IsOverlayActive = false;
		}
#endregion
	}
}
