using ImGuiNET;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DQR.EZInspect
{
	public class LogInspector : MonoBehaviour, IEZInspectionListener
	{
		[SerializeField]
		private Color m_InfoMessageColour = Color.grey;

		[SerializeField]
		private Color m_WarningMessageColour = Color.yellow;

		[SerializeField]
		private Color m_ErrorMessageColour = Color.red;

		public InspectionProperties GetInspectProperties()
		{
			InspectionProperties props = new InspectionProperties();
			props.TitleOverride = "Logger";
			return props;
		}

		public void OnInspect(EZInspector inspector)
		{
			string searchFilter = "TODO";
			ImGui.InputText("Filter", ref searchFilter, 1024);
			
			{
				if (ImGui.BeginCombo("Categories", "", ImGuiComboFlags.NoPreview))
				{
					foreach (var category in Log.GetDiscoveredCategories())
					{
						bool console = category.ShouldLogToConsole;
						ImGui.Checkbox(category.GetCategoryName(), ref console);
						category.ShouldLogToConsole = console;
					}

					ImGui.EndCombo();
				}

				if (ImGui.Button("Copy To Clipboard"))
				{
					TextEditor te = new TextEditor();
				
					foreach (var entry in Log.GetLogMessages())
						te.text += entry.ToString() + '\n';
				
					te.SelectAll();
					te.Copy();
				}
				ImGui.SameLine();
				
				if (ImGui.Button("Clear Log"))
				{
					Log.Clear();
				}
			}

			ImGui.Separator();
			{
				ImGui.Columns(3);

				ImGui.SetColumnWidth(0, 140.0f);
				ImGui.SetColumnWidth(1, 80.0f);
				//ImGui.SetColumnWidth(1, -1);

				int i = 0;
				foreach (var entry in Log.GetLogMessages().Reverse())
				{
					if (!entry.Category.ShouldLogToConsole)
						continue;

					ImGui.PushID("##entry_" + i);
					Color catColour = entry.Category.GetCategoryColour();
					ImGui.TextColored(catColour, entry.Category.GetCategoryName());
					ImGui.NextColumn();

					Color msgColour = m_InfoMessageColour;
					switch (entry.LogType)
					{
						case LogMessageType.Error:
							msgColour = m_ErrorMessageColour;
							break;

						case LogMessageType.Warning:
							msgColour = m_WarningMessageColour;
							break;
					}

					ImGui.Text(entry.Timestamp.ToShortTimeString());
					ImGui.NextColumn();

					ImGui.PushStyleColor(ImGuiCol.Text, msgColour);
					if (ImGui.Selectable(entry.Message))
					{
						// Copy message to clipboard
						TextEditor te = new TextEditor();
						te.text = entry.ToString();
						te.SelectAll();
						te.Copy();
					}
					ImGui.PopStyleColor();
					

					//ImGui.TextColored(msgColour, entry.Message);
					ImGui.NextColumn();
					ImGui.PopID();
				}
			}
		}
	}
}
