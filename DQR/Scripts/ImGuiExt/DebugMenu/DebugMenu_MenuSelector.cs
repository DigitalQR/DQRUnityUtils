using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ImGuiNET;

public class DebugMenu_MenuSelector : DebugMenu
{
	private string m_SearchFilter = "";

	protected override void InitMenu()
	{
		m_InitialPosition = new Vector2(5, 770);
		m_InitialSize = new Vector2(330, 300);
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.BackQuote))
		{
			IsMenuVisible = !IsMenuVisible;
		}
	}

	private bool ContainedInSearchFilter(string name)
	{
		if (string.IsNullOrWhiteSpace(m_SearchFilter))
			return true;

		return m_SearchFilter.Split(' ').Where((filter) => !string.IsNullOrWhiteSpace(filter) && name.IndexOf(filter, System.StringComparison.CurrentCultureIgnoreCase) >= 0).Any();
	}

	protected override void RenderMenuContents()
	{
		if (DebugMenuController.Instance)
		{
			ImGui.InputText("Filter", ref m_SearchFilter, 1024);
			ImGui.Text("Registered " + DebugMenuController.Instance.MenuCount + " menus");
			ImGui.Spacing();

			ImGui.BeginChild("scroll_area");

			foreach (IDebugMenu menu in DebugMenuController.Instance.AllMenus)
			{
				if (menu.Equals(this))
					continue;

				string menuName = menu.MenuName;

				if (ContainedInSearchFilter(menuName))
				{
					bool visible = menu.IsMenuVisible;
					if (ImGui.Checkbox(menu.MenuName, ref visible))
					{
						menu.IsMenuVisible = visible;
					}
				}
			}

			ImGui.EndChild();
		}
	}
}
