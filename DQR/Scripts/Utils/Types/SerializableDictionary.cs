using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace DQR.Types
{
	/// <summary>
	/// Make sure to extend this into a separate type, if you want to serialize using it
	/// </summary>
	[System.Serializable]
	public class SerializableDictionary<TKey, TVal> : Dictionary<TKey, TVal>, ISerializationCallbackReceiver
	{
		[System.Serializable]
		private struct KVP
		{
			public TKey Key;
			public TVal Value;
		}

		[SerializeField]
		private List<KVP> m_Elements = new List<KVP>();
		
//#if 1 UNITY_EDITOR
		[SerializeField, HideInInspector]
		private KVP m_TemporaryKVP = new KVP();

		[SerializeField, HideInInspector]
		private bool m_AppendTemporary = false;
//#endif
		public SerializableDictionary()
		{ }

		public SerializableDictionary(IDictionary<TKey, TVal> dictionary) : base(dictionary)
		{ }

		public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer)
		{ }

		public SerializableDictionary(int capacity) : base(capacity)
		{ }

		public SerializableDictionary(IDictionary<TKey, TVal> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer)
		{ }

		public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer)
		{ }

		public void OverrideFrom(Dictionary<TKey, TVal> dictionary)
		{
			foreach (var pair in dictionary)
			{
				this[pair.Key] = pair.Value;
			}
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			Clear();
#if UNITY_EDITOR
			if (m_AppendTemporary)
			{
				if(m_Elements.Where((kvp) => kvp.Key.Equals(m_TemporaryKVP.Key)).Any())
				{
					Log.Warning<DefaultLogCategory>("Duplicate key added to Dictionary (overriding existing)");
				}

				m_Elements.Add(m_TemporaryKVP);
				m_TemporaryKVP = default;
				m_AppendTemporary = false;
			}
#endif

			for (int i = 0; i < m_Elements.Count; ++i)
			{
				this[m_Elements[i].Key] = m_Elements[i].Value;
			}

			m_Elements.Clear();
			
			foreach(var kvp in this)
			{
				KVP elem = new KVP();
				elem.Key = kvp.Key;
				elem.Value = kvp.Value;
				m_Elements.Add(elem);
			}
		}
	}
}