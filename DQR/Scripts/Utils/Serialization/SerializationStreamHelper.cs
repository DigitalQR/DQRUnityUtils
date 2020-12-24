using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum SerializationMode
{
	Write,
	Read,
}

/// <summary>
/// Method helpers to help with serializations
/// </summary>
public static class SerializationHelper
{
	#region Basic Types

	public static byte Serialize(Stream stream, SerializationMode mode, ref byte value)
	{
		if (mode == SerializationMode.Write)
		{
			stream.WriteByte(value);
		}
		else
		{
			value = (byte)stream.ReadByte();
		}
		return value;
	}

	public static sbyte Serialize(Stream stream, SerializationMode mode, ref sbyte value)
	{
		if (mode == SerializationMode.Write)
		{
			stream.WriteByte((byte)value);
		}
		else
		{
			value = (sbyte)stream.ReadByte();
		}
		return value;
	}

	public static short Serialize(Stream stream, SerializationMode mode, ref short value)
	{
		if (mode == SerializationMode.Write)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, bytes.Length);
		}
		else
		{
			const int size = sizeof(short);
			byte[] bytes = new byte[size];
			int result = stream.Read(bytes, 0, size);

			if (result == size)
				value = BitConverter.ToInt16(bytes, 0);
			else
				Debug.AssertFormat(false, "Serialize cannot read correct count (Got {0}; Expected {1})", result, size);
		}

		return value;
	}

	public static ushort Serialize(Stream stream, SerializationMode mode, ref ushort value)
	{
		if (mode == SerializationMode.Write)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, bytes.Length);
		}
		else
		{
			const int size = sizeof(ushort);
			byte[] bytes = new byte[size];
			int result = stream.Read(bytes, 0, size);

			if (result == size)
				value = BitConverter.ToUInt16(bytes, 0);
			else
				Debug.AssertFormat(false, "Serialize cannot read correct count (Got {0}; Expected {1})", result, size);
		}

		return value;
	}

	public static int Serialize(Stream stream, SerializationMode mode, ref int value)
	{
		if (mode == SerializationMode.Write)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, bytes.Length);
		}
		else
		{
			const int size = sizeof(int);
			byte[] bytes = new byte[size];
			int result = stream.Read(bytes, 0, size);

			if (result == size)
				value = BitConverter.ToInt32(bytes, 0);
			else
				Debug.AssertFormat(false, "Serialize cannot read correct count (Got {0}; Expected {1})", result, size);
		}

		return value;
	}

	public static uint Serialize(Stream stream, SerializationMode mode, ref uint value)
	{
		if (mode == SerializationMode.Write)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, bytes.Length);
		}
		else
		{
			const int size = sizeof(uint);
			byte[] bytes = new byte[size];
			int result = stream.Read(bytes, 0, size);

			if (result == size)
				value = BitConverter.ToUInt32(bytes, 0);
			else
				Debug.AssertFormat(false, "Serialize cannot read correct count (Got {0}; Expected {1})", result, size);
		}

		return value;
	}

	public static long Serialize(Stream stream, SerializationMode mode, ref long value)
	{
		if (mode == SerializationMode.Write)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, bytes.Length);
		}
		else
		{
			const int size = sizeof(long);
			byte[] bytes = new byte[size];
			int result = stream.Read(bytes, 0, size);

			if (result == size)
				value = BitConverter.ToInt64(bytes, 0);
			else
				Debug.AssertFormat(false, "Serialize cannot read correct count (Got {0}; Expected {1})", result, size);
		}

		return value;
	}

	public static ulong Serialize(Stream stream, SerializationMode mode, ref ulong value)
	{
		if (mode == SerializationMode.Write)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, bytes.Length);
		}
		else
		{
			const int size = sizeof(ulong);
			byte[] bytes = new byte[size];
			int result = stream.Read(bytes, 0, size);

			if (result == size)
				value = BitConverter.ToUInt64(bytes, 0);
			else
				Debug.AssertFormat(false, "Serialize cannot read correct count (Got {0}; Expected {1})", result, size);
		}

		return value;
	}

	public static float Serialize(Stream stream, SerializationMode mode, ref float value)
	{
		if (mode == SerializationMode.Write)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, bytes.Length);
		}
		else
		{
			const int size = sizeof(float);
			byte[] bytes = new byte[size];
			int result = stream.Read(bytes, 0, size);

			if (result == size)
				value = BitConverter.ToSingle(bytes, 0);
			else
				Debug.AssertFormat(false, "Serialize cannot read correct count (Got {0}; Expected {1})", result, size);
		}

		return value;
	}

	public static double Serialize(Stream stream, SerializationMode mode, ref double value)
	{
		if (mode == SerializationMode.Write)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, bytes.Length);
		}
		else
		{
			const int size = sizeof(double);
			byte[] bytes = new byte[size];
			int result = stream.Read(bytes, 0, size);

			if (result == size)
				value = BitConverter.ToDouble(bytes, 0);
			else
				Debug.AssertFormat(false, "Serialize cannot read correct count (Got {0}; Expected {1})", result, size);
		}

		return value;
	}

	/// <summary>
	/// Serializes string in UTF8 format unless specified otherwise
	/// </summary>
	public static string Serialize(Stream stream, SerializationMode mode, ref string value, System.Text.Encoding encoding = null)
	{
		if (encoding == null)
			encoding = System.Text.Encoding.UTF8;

		if (mode == SerializationMode.Write)
		{
			byte[] bytes = encoding.GetBytes(value);
			int length = bytes.Length;

			Serialize(stream, mode, ref length);
			stream.Write(bytes, 0, length);
		}
		else
		{
			int length = 0;
			Serialize(stream, mode, ref length);

			byte[] bytes = new byte[length];
			int result = stream.Read(bytes, 0, length);

			if (result == length)
				value = encoding.GetString(bytes);
			else
				Debug.AssertFormat(false, "Serialize cannot read correct count for string (Got {0}; Expected {1})", result, length);
		}

		return value;
	}

	#endregion

	#region Advanced Types

	public static Vector2 Serialize(Stream stream, SerializationMode mode, ref Vector2 value)
	{
		value.x = Serialize(stream, mode, ref value.x);
		value.y = Serialize(stream, mode, ref value.y);
		return value;
	}

	public static Vector2Int Serialize(Stream stream, SerializationMode mode, ref Vector2Int value)
	{
		int x = value.x;
		int y = value.y;
		value.x = Serialize(stream, mode, ref x);
		value.y = Serialize(stream, mode, ref y);
		return value;
	}

	public static Vector3 Serialize(Stream stream, SerializationMode mode, ref Vector3 value)
	{
		value.x = Serialize(stream, mode, ref value.x);
		value.y = Serialize(stream, mode, ref value.y);
		value.z = Serialize(stream, mode, ref value.z);
		return value;
	}

	public static Vector3Int Serialize(Stream stream, SerializationMode mode, ref Vector3Int value)
	{
		int x = value.x;
		int y = value.y;
		int z = value.z;
		value.x = Serialize(stream, mode, ref x);
		value.y = Serialize(stream, mode, ref y);
		value.z = Serialize(stream, mode, ref z);
		return value;
	}

	public static Vector4 Serialize(Stream stream, SerializationMode mode, ref Vector4 value)
	{
		value.x = Serialize(stream, mode, ref value.x);
		value.y = Serialize(stream, mode, ref value.y);
		value.z = Serialize(stream, mode, ref value.z);
		value.w = Serialize(stream, mode, ref value.w);
		return value;
	}

	#endregion

	#region Extended Serialization

	public static byte[] SerializeBlock(Stream stream, SerializationMode mode, ref byte[] block)
	{
		int length = (mode == SerializationMode.Write) ? block.Length : 0;
		Serialize(stream, mode, ref length);

		if (mode == SerializationMode.Write)
		{
			stream.Write(block, 0, length);
		}
		else
		{
			block = new byte[length];
			int read = stream.Read(block, 0, length);
			Debug.AssertFormat(read == length, "Error when reading block; read count ({0}) doesn't match expected block size ({1}))", read, length);
		}

		return block;
	}

	public static byte[] SerializeBlock(Stream stream, SerializationMode mode, ref string blockName, ref byte[] block)
	{
		Serialize(stream, mode, ref blockName);

		int length = (mode == SerializationMode.Write) ? block.Length : 0;
		Serialize(stream, mode, ref length);

		if (mode == SerializationMode.Write)
		{
			stream.Write(block, 0, length);
		}
		else
		{
			block = new byte[length];
			int read = stream.Read(block, 0, length);
			Debug.AssertFormat(read == length, "Error when reading block '{0}'; read count ({1}) doesn't match expected block size ({2}))", blockName, read, length);
		}

		return block;
	}

	#endregion
}


/// <summary>
/// Wrapper for a stream to safely track the serialization mode for helpers above
/// </summary>
public class SerializationStream : IDisposable
{
	private Stream m_Stream;
	private SerializationMode m_Mode;

	public SerializationStream(Stream stream, SerializationMode mode)
	{
		m_Stream = stream;
		m_Mode = mode;
	}

	public Stream BaseStream
	{
		get => m_Stream;
	}

	public SerializationMode Mode
	{
		get => m_Mode;
	}

	public void Dispose()
	{
		m_Stream.Dispose();
	}

	public byte Serialize(ref byte value)
	{
		return SerializationHelper.Serialize(m_Stream, m_Mode, ref value);
	}

	public sbyte Serialize(ref sbyte value)
	{
		return SerializationHelper.Serialize(m_Stream, m_Mode, ref value);
	}

	public short Serialize(ref short value)
	{
		return SerializationHelper.Serialize(m_Stream, m_Mode, ref value);
	}

	public ushort Serialize(ref ushort value)
	{
		return SerializationHelper.Serialize(m_Stream, m_Mode, ref value);
	}

	public int Serialize(ref int value)
	{
		return SerializationHelper.Serialize(m_Stream, m_Mode, ref value);
	}

	public uint Serialize(ref uint value)
	{
		return SerializationHelper.Serialize(m_Stream, m_Mode, ref value);
	}

	public long Serialize(ref long value)
	{
		return SerializationHelper.Serialize(m_Stream, m_Mode, ref value);
	}

	public ulong Serialize(ref ulong value)
	{
		return SerializationHelper.Serialize(m_Stream, m_Mode, ref value);
	}

	public float Serialize(ref float value)
	{
		return SerializationHelper.Serialize(m_Stream, m_Mode, ref value);
	}

	public double Serialize(ref double value)
	{
		return SerializationHelper.Serialize(m_Stream, m_Mode, ref value);
	}

	public string Serialize(ref string value, System.Text.Encoding encoding = null)
	{
		return SerializationHelper.Serialize(m_Stream, m_Mode, ref value, encoding);
	}

	public Vector2 Serialize(ref Vector2 value)
	{
		return SerializationHelper.Serialize(m_Stream, m_Mode, ref value);
	}

	public Vector2Int Serialize(ref Vector2Int value)
	{
		return SerializationHelper.Serialize(m_Stream, m_Mode, ref value);
	}

	public Vector3 Serialize(ref Vector3 value)
	{
		return SerializationHelper.Serialize(m_Stream, m_Mode, ref value);
	}

	public Vector3Int Serialize(ref Vector3Int value)
	{
		return SerializationHelper.Serialize(m_Stream, m_Mode, ref value);
	}

	public Vector4 Serialize(ref Vector4 value)
	{
		return SerializationHelper.Serialize(m_Stream, m_Mode, ref value);
	}

	public byte[] SerializeBlock(ref byte[] block)
	{
		return SerializationHelper.SerializeBlock(m_Stream, m_Mode, ref block);
	}

	public byte[] SerializeBlock(ref string blockName, ref byte[] block)
	{
		return SerializationHelper.SerializeBlock(m_Stream, m_Mode, ref blockName, ref block);
	}
}