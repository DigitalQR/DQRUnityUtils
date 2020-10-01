using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDebugMenu
{
	string MenuName { get; }

	bool IsMenuVisible { get; set; }

	void OnRenderMenu(DebugMenuController controller);
}