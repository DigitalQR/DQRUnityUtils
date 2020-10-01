using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DQR.Types
{
	[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
	public class SubTypeOfAttribute : PropertyAttribute
	{
		private System.Type m_BaseType;

		public SubTypeOfAttribute(System.Type baseType)
		{
			m_BaseType = baseType;
		}

		public System.Type BaseType
		{
			get => m_BaseType;
		}

		public IEnumerable<string> GetAssignableTypes()
		{
			IEnumerable<string> results = new string[0];

			if (m_BaseType != null)
			{
				foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
				{
					results = results
						.Union(assembly.GetTypes().Where((t) => m_BaseType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
						.Select((t) => t.FullName)
						);
				}
			}

			return results;
		}
	}
}