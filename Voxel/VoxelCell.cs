using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace DQR.Voxel
{
	[StructLayout(LayoutKind.Explicit)]
	public struct VoxelCell
	{
		public static readonly VoxelCell Invalid = new VoxelCell(uint.MinValue);

		[FieldOffset(0)]
		public uint m_uint32;

		[FieldOffset(0)]
		public int m_int32;


		[FieldOffset(0)]
		public ushort m_uint16_0;
		[FieldOffset(2)]
		public ushort m_uint16_1;

		[FieldOffset(0)]
		public short m_int16_0;
		[FieldOffset(2)]
		public short m_int16_1;


		[FieldOffset(0)]
		public byte m_uint8_0;
		[FieldOffset(1)]
		public byte m_uint8_1;
		[FieldOffset(2)]
		public byte m_uint8_2;
		[FieldOffset(3)]
		public byte m_uint8_3;

		[FieldOffset(0)]
		public sbyte m_int8_0;
		[FieldOffset(1)]
		public sbyte m_int8_1;
		[FieldOffset(2)]
		public sbyte m_int8_2;
		[FieldOffset(3)]
		public sbyte m_int8_3;

		public VoxelCell(uint uint32)
		{
			// Gross, have to set all values first
			m_uint32 = 0;
			m_int32 = 0;

			m_uint16_0 = 0;
			m_uint16_1 = 0;
			m_int16_0 = 0;
			m_int16_1 = 0;

			m_uint8_0 = 0;
			m_uint8_1 = 0;
			m_uint8_2 = 0;
			m_uint8_3 = 0;
			m_int8_0 = 0;
			m_int8_1 = 0;
			m_int8_2 = 0;
			m_int8_3 = 0;

			m_uint32 = uint32;
		}

		public VoxelCell(int int32)
		{
			// Gross, have to set all values first
			m_uint32 = 0;
			m_int32 = 0;

			m_uint16_0 = 0;
			m_uint16_1 = 0;
			m_int16_0 = 0;
			m_int16_1 = 0;

			m_uint8_0 = 0;
			m_uint8_1 = 0;
			m_uint8_2 = 0;
			m_uint8_3 = 0;
			m_int8_0 = 0;
			m_int8_1 = 0;
			m_int8_2 = 0;
			m_int8_3 = 0;

			m_int32 = int32;
		}

		public override bool Equals(object obj)
		{
			if (obj is VoxelCell)
			{
				var other = (VoxelCell)obj;
				return m_uint32 == other.m_uint32;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return m_uint32.GetHashCode();
		}

		public static bool operator ==(VoxelCell a, VoxelCell b)
		{
			return a.m_uint32 == b.m_uint32;
		}

		public static bool operator !=(VoxelCell a, VoxelCell b)
		{
			return a.m_uint32 != b.m_uint32;
		}
	}
}
