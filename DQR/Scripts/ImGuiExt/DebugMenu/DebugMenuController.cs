using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ImGuiNET;
using ImGuiNET.Unity;

#if UNITY_EDITOR
using UnityEditor;
#endif

using DQR.Types;

public class DebugMenuController : SingletonBehaviour<DebugMenuController>
{	
	[SerializeField]
	private float m_GlobalScale = 1.0f;

	[SerializeField]
	private bool m_ApplyResolutionScale = true;

	[SerializeField]
	private float m_BaseResolutionWidth = 1920;

	private bool m_ResolutionJustChanged = false;
	private Dictionary<string, IDebugMenu> m_Menus = new Dictionary<string, IDebugMenu>();

	protected override void SingletonInit()
	{
		ImGuiUn.Layout += OnImGuiLayout;

#if UNITY_EDITOR
		EditorApplication.hierarchyChanged += OnEditorHierarchyChanged;
#endif
	}

	public IEnumerable<IDebugMenu> AllMenus
	{
		get => m_Menus.Values;
	}

	public int MenuCount
	{
		get => m_Menus.Count;
	}

	public void RegisterMenu(IDebugMenu menu)
	{
		Debug.Log("DebugMenu: Registered '" + menu.MenuName + "'");
		m_Menus.Add(menu.MenuName, menu);
	}

	public bool DeregisterMenu(IDebugMenu menu)
	{
		Debug.Log("DebugMenu: Deregistered '" + menu.MenuName + "'");
		return m_Menus.Remove(menu.MenuName);
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

		foreach (IDebugMenu menu in m_Menus.Values)
		{
			if (menu.IsMenuVisible)
			{
				menu.OnRenderMenu(this);
			}
		}
	}

#if UNITY_EDITOR
	private void OnEditorHierarchyChanged()
	{
		if (EditorApplication.isPlaying)
		{
			// Auto create a debug menu inspector for each gameobject
			foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
			{
				bool hasInspector = obj.GetComponent<IDebugMenuInspector>() != null;

				if (hasInspector)
				{
					bool hasMenu = obj.GetComponent<DebugMenuInspector>() != null;
					if (!hasMenu)
					{
						obj.AddComponent<DebugMenuInspector>();
					}
				}
			}
		}
	}
#endif

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
}
