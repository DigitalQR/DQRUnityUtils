using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.EZInspect
{
	public class InspectionProperties
	{
		public string TitleOverride;
		public Vector2 InitialPosition;
		public Vector2 InitialSize;
		public bool ManuallyHandleAllRendering;
		public bool IsHiddenByDefault;

		public InspectionProperties()
		{
			TitleOverride = null;
			InitialPosition = new Vector2(5, 5);
			InitialSize = new Vector2(560, 310);
			ManuallyHandleAllRendering = false;
			IsHiddenByDefault = true;
		}
	}

	public interface IEZInspectionListener
	{
		InspectionProperties GetInspectProperties();
		void OnInspect(EZInspector inspector);
	}
}