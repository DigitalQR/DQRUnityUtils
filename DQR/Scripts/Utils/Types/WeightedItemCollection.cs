using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using DQR.Debug;

namespace DQR.Types
{
	[System.Serializable]
	public class WeightedItemCollection<T>
	{
		[System.Serializable]
		public class WeightedItem
		{
			[SerializeField]
			private T m_Item = default;
			
			[SerializeField, Min(1)]
			private int m_Weight = 1;
			
			public T Item
			{
				get => m_Item;
			}

			public int Weight
			{
				get => Mathf.Max(1, m_Weight);
			}
		}
		
		[SerializeField]
		private WeightedItem[] m_Elements = null;
		
		public int TotalWeightSum
		{
			get => m_Elements.Select((w) => w.Weight).Sum();
		}

		public IEnumerable<WeightedItem> AllWeightedElements
		{
			get => m_Elements;
		}

		public T SelectNext()
		{
			if (m_Elements.Length == 0)
				return default;

			int rand = Random.Range(0, TotalWeightSum + 1);
			int v = rand;
			foreach (var elem in m_Elements)
			{
				if (v < elem.Weight)
					return elem.Item;

				v -= elem.Weight;
			}
			
			Assert.FailMessage($"Reached end when attempting to select item from weighted group {rand}/{TotalWeightSum}");
			return default;
		}
	}
	
	[System.Serializable]
	public class WeightedGroupCollection<T>
	{
		[System.Serializable]
		public class WeightedGroup
		{
			[SerializeField]
			private WeightedItemCollection<T> m_Collection = new WeightedItemCollection<T>();

			[SerializeField, Min(1)]
			private int m_Weight = 1;

			public WeightedItemCollection<T> Collection
			{
				get => m_Collection;
			}

			public int Weight
			{
				get => Mathf.Max(1, m_Weight);
			}
		}

		[SerializeField]
		private WeightedItemCollection<T> m_PrimaryGroup = null;

		[SerializeField]
		private WeightedGroup[] m_Subgroups = null;

		public int TotalWeightSum
		{
			get => m_PrimaryGroup.TotalWeightSum + m_Subgroups.Select((w) => w.Weight).Sum();
		}

		public T SelectNext()
		{
			int rand = Random.Range(0, TotalWeightSum + 1);
			int v = rand;
			foreach (var elem in m_PrimaryGroup.AllWeightedElements)
			{
				if (v < elem.Weight)
					return elem.Item;

				v -= elem.Weight;
			}

			foreach (var elem in m_Subgroups)
			{
				if (v < elem.Weight)
					return elem.Collection.SelectNext();

				v -= elem.Weight;
			}

			Assert.FailMessage($"Reached end when attempting to select item from weighted group {rand}/{TotalWeightSum}");
			return default;
		}
	}
}
