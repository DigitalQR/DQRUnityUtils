using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Database
{
	[System.Serializable]
	public struct DBIndex
	{
		[SerializeField]
		private uint m_Offset;

		public DBIndex(uint offset)
		{
			m_Offset = offset;
		}

		public DBIndex(int offset)
		{
			m_Offset = (uint)offset;
		}

		public static bool operator ==(DBIndex a, DBIndex b)
		{
			return a.m_Offset == b.m_Offset;
		}

		public static bool operator !=(DBIndex a, DBIndex b)
		{
			return a.m_Offset != b.m_Offset;
		}

		public static bool operator >(DBIndex a, DBIndex b)
		{
			return a.m_Offset > b.m_Offset;
		}

		public static bool operator <(DBIndex a, DBIndex b)
		{
			return a.m_Offset < b.m_Offset;
		}

		public static bool operator >=(DBIndex a, DBIndex b)
		{
			return a.m_Offset >= b.m_Offset;
		}

		public static bool operator <=(DBIndex a, DBIndex b)
		{
			return a.m_Offset <= b.m_Offset;
		}

		public uint Offset
		{
			get { return m_Offset; }
		}

		public static DBIndex Invalid
		{
			get { return new DBIndex(uint.MaxValue); }
		}

		public override bool Equals(object obj)
		{
			if (obj is DBIndex)
				return m_Offset == ((DBIndex)obj).m_Offset;
			else
				return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return m_Offset.GetHashCode();
		}

		public override string ToString()
		{
			return m_Offset.ToString();
		}
	}
}