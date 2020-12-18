using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ImGuiNET;

public class DebugMenu : MonoBehaviour, IDebugMenu
{
	protected string m_MenuName;
	private bool m_IsMenuVisible;
	
	protected Vector2 m_InitialPosition;
	protected Vector2 m_InitialSize;
	
	public DebugMenu()
	{
		m_MenuName = GetCleanName(GetType());
		m_IsMenuVisible = false;
		m_InitialPosition = new Vector2(5, 5);
		m_InitialSize = new Vector2(560, 310);
	}

	public static string GetCleanName(System.Type type)
	{
		return GetCleanName(type.Name);
	}

	public static string GetCleanName(string rawName)
	{
		if (rawName.StartsWith("DebugMenu_", System.StringComparison.CurrentCultureIgnoreCase))
		{
			rawName = rawName.Substring("DebugMenu_".Length);
		}
		else if (rawName.StartsWith("Dbg_", System.StringComparison.CurrentCultureIgnoreCase))
		{
			rawName = rawName.Substring("Dgb_".Length);
		}

		return rawName;
	}

	public string MenuName
	{
		get => m_MenuName;
	}

	public bool IsMenuVisible
	{
		get => m_IsMenuVisible;
		set => m_IsMenuVisible = value;
	}

	public virtual void Start()
	{
		InitMenu();
		DebugMenuController.Instance.RegisterMenu(this);
	}

	public virtual void OnDestroy()
	{
		if(DebugMenuController.IsValid)
			DebugMenuController.Instance.DeregisterMenu(this);
	}
	
	public void OnRenderMenu(DebugMenuController controller)
	{		
		if (controller.BeginWindow(m_MenuName + "##" + GetType().Name, m_InitialPosition, m_InitialSize))
		{
			m_IsMenuVisible = true;
			RenderMenuContents();
			controller.EndWindow();
		}
		else
		{
			m_IsMenuVisible = false;
		}
	}

	protected virtual void InitMenu()
	{
	}

	protected virtual void RenderMenuContents()
	{
	}
}
