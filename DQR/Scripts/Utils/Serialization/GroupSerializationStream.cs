using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// A stream for serializing group data
/// </summary>
public class GroupSerializationStream : IDisposable
{
	private SerializationStream m_BaseStream;
	private Dictionary<string, MemoryStream> m_SubGroups;

	public GroupSerializationStream(SerializationStream stream)
	{
		m_BaseStream = stream;
		Init();
	}

	public GroupSerializationStream(Stream stream, SerializationMode mode)
	{
		m_BaseStream = new SerializationStream(stream, mode);
		Init();
	}

	public Stream BaseStream
	{
		get => m_BaseStream.BaseStream;
	}

	public SerializationMode Mode
	{
		get => m_BaseStream.Mode;
	}

	private void Init()
	{
		m_SubGroups = new Dictionary<string, MemoryStream>();

		if (Mode == SerializationMode.Read)
			Serialize();
	}
	
	public void Dispose()
	{
		if (Mode == SerializationMode.Write)
			Serialize();

		m_BaseStream.Dispose();
	}

	private void Serialize()
	{
		const int c_InternalFormat = 0;
		int format = c_InternalFormat;
		m_BaseStream.Serialize(ref format);
		
		// Serialize group names and sizes
		//
		int groupCount = m_SubGroups.Count;
		m_BaseStream.Serialize(ref groupCount);

		string[] currentKeys = m_SubGroups.Keys.ToArray();
		
		for (int i = 0; i < groupCount; ++i)
		{
			string groupName = "";
			byte[] groupData = null;
			
			if (Mode == SerializationMode.Write)
			{
				groupName = currentKeys[i];
				groupData = m_SubGroups[groupName].ToArray();
			}

			m_BaseStream.SerializeBlock(ref groupName, ref groupData);

			if (Mode == SerializationMode.Read)
			{
				m_SubGroups[groupName] = new MemoryStream(groupData);
			}
		}
	}

	public bool OpenStream(string key, out Stream stream)
	{
		if (Mode == SerializationMode.Read)
		{
			if (m_SubGroups.TryGetValue(key, out MemoryStream mem))
			{
				stream = mem;
				return true;
			}
		}
		else
		{
			MemoryStream newMem = new MemoryStream();
			m_SubGroups.Add(key, newMem);

			stream = newMem;
			return true;
		}

		stream = null;
		return false;
	}

	public bool OpenSerializationStream(string key, out SerializationStream stream)
	{
		if (OpenStream(key, out Stream subStream))
		{
			stream = new SerializationStream(subStream, Mode);
			return true;
		}

		stream = null;
		return false;
	}

	public bool OpenGroup(string key, out GroupSerializationStream stream)
	{
		if (OpenStream(key, out Stream subStream))
		{
			stream = new GroupSerializationStream(subStream, Mode);
			return true;
		}
		
		stream = null;
		return false;
	}
}
