using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DQR.Types;

namespace DQR.EZInspect
{
	public class EZInspectionContainer : MonoBehaviour
	{
		private IEZInspectionListener[] m_Listeners = null;

		private void Start()
		{
			if (EZInspector.IsValid)
			{
				m_Listeners = GetComponents<IEZInspectionListener>();

				foreach (var listener in m_Listeners)
					EZInspector.Instance.RegisterListener(gameObject, listener);
			}
		}

		private void OnDestroy()
		{
			if (EZInspector.IsValid && m_Listeners != null)
			{
				foreach (var listener in m_Listeners)
					EZInspector.Instance.UnregisterListener(listener);
			}
		}
	}

	public class EZInspectionListenerContainer
	{
		private IEZInspectionListener m_Listener;
		private InspectionProperties m_Props;

		private PropertyPrefBool m_IsInspectorActive;
		private string m_FullTitle;
		private string m_ShortTitle;

		public EZInspectionListenerContainer(GameObject context, IEZInspectionListener listener)
		{
			m_Listener = listener;
			m_Props = m_Listener.GetInspectProperties();

			if (string.IsNullOrWhiteSpace(m_Props.TitleOverride))
				m_ShortTitle = listener.GetType().Name;
			else
				m_ShortTitle = m_Props.TitleOverride;

			m_FullTitle = context.name + "." + m_ShortTitle;
			m_IsInspectorActive = new PropertyPrefBool("DQR.EZInspect." + m_FullTitle + ".Active", m_Props.IsHiddenByDefault);
		}

		public string FullTitle
		{
			get => m_FullTitle;
		}

		public string ShortTitle
		{
			get => m_ShortTitle;
		}
		
		public bool IsActive
		{
			get => m_IsInspectorActive;
			set => m_IsInspectorActive.Set(value);
		}

		public void OnInspect(EZInspector inspector)
		{
			if (m_Props.ManuallyHandleAllRendering)
			{
				m_Listener.OnInspect(inspector);
			}
			else
			{
				if (inspector.BeginWindow(m_ShortTitle + "##" + m_FullTitle, m_Props.InitialPosition, m_Props.InitialSize))
				{
					m_Listener.OnInspect(inspector);
				}
				else
				{
					IsActive = false;
				}
				inspector.EndWindow();
			}
		}
	}
}
