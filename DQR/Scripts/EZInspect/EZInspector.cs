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
		private float m_GlobalScale = 1.0f;

		[SerializeField]
		private bool m_ApplyResolutionScale = true;

		[SerializeField]
		private float m_BaseResolutionWidth = 1920;

		private bool m_ResolutionJustChanged = false;

		private Dictionary<IEZInspectionListener, EZInspectionListenerContainer> m_InspectListeners = new Dictionary<IEZInspectionListener, EZInspectionListenerContainer>();
		
		protected override void SingletonInit()
		{
			SetupListenerContainerIfRequired(gameObject);
			ImGuiUn.Layout += OnImGuiLayout;

#if UNITY_EDITOR
			EditorApplication.hierarchyChanged += OnEditorHierarchyChanged;
#endif
		}

		public void Update()
		{
			// Toggle with '`'
			if (Input.GetKeyDown(KeyCode.BackQuote))
			{
				m_InspectListeners.TryGetValue(this, out EZInspectionListenerContainer container);
				container.IsActive = !container.IsActive;
			}
		}

		public float RenderResolutionScale
		{
			get
			{
				return m_ApplyResolutionScale ? (float)Screen.width / m_BaseResolutionWidth : 1.0f;
			}
		}

		public float RenderGlobalScale
		{
			get
			{
				return m_GlobalScale * RenderResolutionScale;
			}
		}

		private void OnImGuiLayout()
		{
			// Use hidden window to detect when resolution just changed
			{
				bool justChanged = m_ResolutionJustChanged;
				m_ResolutionJustChanged = false;

				Vector2 currentResolution = new Vector2(Screen.width, Screen.height);

				ImGui.SetNextWindowCollapsed(false, ImGuiCond.Always);
				ImGui.SetNextWindowPos(currentResolution * 4, ImGuiCond.Always);
				ImGui.SetNextWindowSize(currentResolution, justChanged ? ImGuiCond.Always : ImGuiCond.FirstUseEver);

				bool open = true;
				if (ImGui.Begin("##res_tracker", ref open))
				{
					Vector2 previousResolution = ImGui.GetWindowSize();

					if (currentResolution != previousResolution)
					{
						Debug.Log("DebugMenu: Resolution change detected");
						m_ResolutionJustChanged = true;
					}

					ImGui.End();
				}
			}

			// Render overlays
			ImGui.GetIO().FontGlobalScale = RenderGlobalScale;

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


		public bool BeginWindow(string name, Vector2 initPos, Vector2 initSize)
		{
			ImGuiCond propCondition = m_ResolutionJustChanged ? ImGuiCond.Always : ImGuiCond.FirstUseEver;

			ImGui.SetNextWindowCollapsed(false, ImGuiCond.Always);
			ImGui.SetNextWindowPos(initPos * RenderResolutionScale, propCondition);
			ImGui.SetNextWindowSize(initSize * RenderResolutionScale, propCondition);

			bool open = true;
			return ImGui.Begin(name, ref open) && open;
		}

		public void EndWindow()
		{
			ImGui.End();
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

			BeginWindow("EZInspector", new Vector2(5, 770), new Vector2(330, 300));
			{
				ImGui.InputText("Filter", ref m_SearchFilter, 1024);
				ImGui.Text("Registered " + (m_InspectListeners.Count - 1) + " inspectors");
				ImGui.Spacing();

				ImGui.BeginChild("scroll_area");

				foreach (var inspect in m_InspectListeners.Values)
				{
					if (inspect.Equals(this))
						continue;

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
			}
			ImGui.EndChild();

			EndWindow();
		}
		#endregion
	}
}
