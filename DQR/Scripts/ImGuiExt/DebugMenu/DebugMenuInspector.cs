using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ImGuiNET;


// Debug inspector callback for an object which is avaliable in all builds supporting debug menus
//
public interface IDebugMenuInspector
{
	void OnRenderDebugInspect();
}

// Debug inspector callback for an object which is only avaliable in development builds
//
public interface IDebugMenuInspectorDEV
#if UNITY_EDITOR
: IDebugMenuInspector
#endif
{
}

public class DebugMenuInspector : DebugMenu
{
	protected override void InitMenu()
	{
		m_MenuName = "Inspector:" + gameObject.name;
		m_InitialSize = new Vector2(350, 425);
	}

	protected override void RenderMenuContents()
	{
		int counter = 0;

		foreach (IDebugMenuInspector inspector in gameObject.GetComponents<IDebugMenuInspector>())
		{
			string prefix = "";
#if UNITY_EDITOR
			if (inspector is IDebugMenuInspectorDEV)
				prefix = "DEV:";
#endif
			
			if (ImGui.CollapsingHeader(prefix + GetCleanName(inspector.GetType()) + "##inspector_" + (counter++)))
				inspector.OnRenderDebugInspect();
		}
	}
}