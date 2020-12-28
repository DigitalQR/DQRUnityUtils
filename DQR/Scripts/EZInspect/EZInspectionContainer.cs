using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
			get => PlayerPrefs.GetInt("DQR.EZInspect." + m_FullTitle + ".Active", m_Props.IsHiddenByDefault ? 0 : 1) == 1;
			set
			{
				PlayerPrefs.SetInt("DQR.EZInspect." + m_FullTitle + ".Active", value ? 1 : 0);
				Log.Info<EZInspectLogCategory>("'{0}' active:{1}", m_FullTitle, value);
			}
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
