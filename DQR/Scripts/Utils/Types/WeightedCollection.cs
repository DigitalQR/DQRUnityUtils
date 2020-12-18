using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DQR.Types
{
	[System.Serializable]
	public class WeightedCollection<T> : ISerializationCallbackReceiver
	{
		public struct WeightedItem
		{
			public float Weight;
			public T Item;

			public override string ToString()
			{
				return $"{Weight}:{Item.ToString()}";
			}
		}

		public struct WeightedSubcollection
		{
			public float Weight;
			public WeightedCollection<T> Collection;

			public override string ToString()
			{
				return $"{Weight}:{Collection.ToString()}";
			}
		}

		private List<WeightedItem> m_Items;
		private List<WeightedSubcollection> m_Subcollections;
		private bool m_IsDirty;

		public WeightedCollection()
		{
			m_Items = new List<WeightedItem>();
			m_Subcollections = new List<WeightedSubcollection>();
			m_IsDirty = false;
		}

		public WeightedCollection(WeightedCollection<T> other)
		{
			m_Items = new List<WeightedItem>(other.m_Items);
			m_Subcollections = new List<WeightedSubcollection>(other.m_Subcollections);
		}

		public float TotalWeight
		{
			get => m_Items.Sum((i) => i.Weight) + m_Subcollections.Sum((i) => i.Weight);
		}

		public IEnumerable<WeightedItem> GetNormalizedWeightedItems()
		{
			return GetScaledWeightedItems(1.0f);
		}

		private IEnumerable<WeightedItem> GetScaledWeightedItems(float weightScale)
		{
			float totalWeight = TotalWeight;
			IEnumerable<WeightedItem> items = m_Items.Select((i) => new WeightedItem { Weight = i.Weight * weightScale / totalWeight, Item = i.Item });

			foreach (var elem in m_Subcollections)
			{
				float elemWeight = elem.Weight * weightScale / totalWeight;
				items = items.Union(elem.Collection.GetScaledWeightedItems(elemWeight));
			}

			return items;
		}

		public T SelectValue(float t)
		{
			var allItems = GetNormalizedWeightedItems();
			float totalWeight = allItems.Sum((i) => i.Weight);

			float v = t * totalWeight;
			return SelectItemInternal(allItems, v);
		}

		public T SelectRandom()
		{
			var allItems = GetNormalizedWeightedItems();
			float totalWeight = allItems.Sum((i) => i.Weight);

			float v = Random.Range(0.0f, totalWeight);
			return SelectItemInternal(allItems, v);
		}

		private static T SelectItemInternal(IEnumerable<WeightedItem> items, float v)
		{
			foreach (var elem in items)
			{
				if (v <= elem.Weight)
					return elem.Item;

				v -= elem.Weight;
			}

			return default;
		}

		public void Insert(float weight, T item)
		{
			m_Items.Add(new WeightedItem { Weight = weight, Item = item });
			m_IsDirty = true;
		}

		public void Insert(float weight, WeightedCollection<T> collection)
		{
			m_Subcollections.Add(new WeightedSubcollection { Weight = weight, Collection = collection });
			m_IsDirty = true;
		}

		public void InsertRange(float weightPerItem, IEnumerable<T> items)
		{
			foreach (T item in items)
				Insert(weightPerItem, item);
		}

		public static WeightedCollection<T> Lerp(WeightedCollection<T> a, WeightedCollection<T> b, float t)
		{
			if (t <= 0.0f)
				return a;
			else if (t >= 1.0f)
				return b;
			else
			{
				WeightedCollection<T> output = new WeightedCollection<T>();

				foreach (var elem in a.GetScaledWeightedItems(1.0f - t))
					output.Insert(elem.Weight, elem.Item);

				foreach (var elem in b.GetScaledWeightedItems(t))
					output.Insert(elem.Weight, elem.Item);

				return output;
			}
		}

		[System.Serializable]
		private struct SerializedItem
		{
			[Min(0)]
			public float Weight;
			public T Item;
		}

		[System.Serializable]
		private struct SerializedCollection
		{
			[Min(0)]
			public float Weight;
			public List<SerializedItem> Items;
			public List<int> SubcollectionIndices;
		}

		[SerializeField]
		private List<SerializedCollection> m_SerializedCollections = null;
		
		private SerializedCollection WriteSerializedVars(List<SerializedCollection> target)
		{
			SerializedCollection root = new SerializedCollection();
			root.Weight = 1.0f;
			root.Items = new List<SerializedItem>();
			root.SubcollectionIndices = new List<int>();

			foreach (var elem in m_Items)
				root.Items.Add(new SerializedItem { Weight = elem.Weight, Item = elem.Item });

			foreach (var elem in m_Subcollections)
			{
				root.SubcollectionIndices.Add(target.Count + 1);

				SerializedCollection collection = elem.Collection.WriteSerializedVars(target);
				collection.Weight = elem.Weight;
			}

			return root;
		}

		private SerializedCollection ReadSerializedVars(int index, List<SerializedCollection> target)
		{
			SerializedCollection root = target[index];
			m_Items = new List<WeightedItem>();
			m_Subcollections = new List<WeightedSubcollection>();
			
			foreach (var elem in root.Items)
				m_Items.Add(new WeightedItem { Weight = elem.Weight, Item = elem.Item });

			foreach (int i in root.SubcollectionIndices)
			{
				WeightedSubcollection elem = new WeightedSubcollection();
				elem.Collection = new WeightedCollection<T>();
				elem.Weight = elem.Collection.ReadSerializedVars(i, target).Weight;

				m_Subcollections.Add(elem);
			}
			
			return root;
		}
		
		public void OnBeforeSerialize()
		{
			if (m_IsDirty)
			{
				m_IsDirty = false;

				m_SerializedCollections = new List<SerializedCollection>();
				WriteSerializedVars(m_SerializedCollections);
			}
		}

		public void OnAfterDeserialize()
		{
			if (m_SerializedCollections == null)
				m_SerializedCollections = new List<SerializedCollection>();

			if (m_SerializedCollections.Count == 0)
			{
				SerializedCollection root = new SerializedCollection();
				root.Weight = 1.0f;
				root.Items = new List<SerializedItem>();
				root.SubcollectionIndices = new List<int>();
				m_SerializedCollections.Add(root);
			}

			ReadSerializedVars(0, m_SerializedCollections);
		}
	}
}
