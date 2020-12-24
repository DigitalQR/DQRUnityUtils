using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

/// <summary>
/// Wrapper for serializer which will serialize all data given into blocks with unique keys
/// </summary> TODOFIX
/*
public class SerializationChunk
{
	private Dictionary<string, MemoryStream> m_ChunkTable;

	public SerializationChunk()
	{
		m_ChunkTable = new Dictionary<string, MemoryStream>();
	}

	public int ChunkCount
	{
		get => m_ChunkTable.Count;
	}

	public bool IsEmpty
	{
		get => ChunkCount == 0;
	}

	/// <summary>
	/// Does this chunk have any data in any of the chunks
	/// </summary>
	public bool HasAnyData
	{
		get => !IsEmpty && m_ChunkTable.Where((kvp) => kvp.Value.Length != 0).Any();
	}

	public IEnumerable<string> ChunkNames
	{
		get => m_ChunkTable.Keys;
	}

	/// <summary>
	/// Deserialize chunks from input stream
	/// </summary>
	public void ImportChunks(Stream stream)
	{
		Debug.Assert(stream.CanRead, "Cannot import blocks from unreadable stream");
		if (stream.CanRead)
		{
			int blockCount = 0;
			SerializationHelper.Serialize(stream, SerializationMode.Read, ref blockCount);

			for (int i = 0; i < blockCount; ++i)
			{
				string chunkName = "";
				byte[] content = null;
				SerializationHelper.SerializeBlock(stream, SerializationMode.Read, ref chunkName, ref content);

				Debug.AssertFormat(!m_ChunkTable.ContainsKey(chunkName), "Imported chunk name '{0}' already exists with in collection", chunkName);
				m_ChunkTable[chunkName] = new MemoryStream(content);
			}
		}
	}

	/// <summary>
	/// Export chunks to output stream
	/// </summary>
	public void ExportChunks(Stream stream)
	{
		Debug.Assert(stream.CanWrite, "Cannot import chunks from unwritable stream");
		if (stream.CanWrite)
		{
			int blockCount = m_ChunkTable.Count;
			SerializationHelper.Serialize(stream, SerializationMode.Write, ref blockCount);

			foreach (var kvp in m_ChunkTable)
			{
				string chunkName = kvp.Key;
				byte[] content = kvp.Value.ToArray();
				SerializationHelper.SerializeBlock(stream, SerializationMode.Write, ref chunkName, ref content);
			}
		}
	}

	/// <summary>
	/// Attempt to find or create (If the mode allows it) a stream for the given chunk
	/// </summary>
	public bool TryGetChunk(string name, SerializationMode mode, out MemoryStream stream)
	{
		// In write mode, always overwrite existing stream
		if (mode == SerializationMode.Write)
		{
			//if (m_ChunkTable.ContainsKey(name))
			//	Debug.LogWarningFormat("Overwriting existing chunk for '{0}'", name);

			stream = new MemoryStream();
			m_ChunkTable[name] = stream;
			return true;
		}
		else
		{
			if (m_ChunkTable.TryGetValue(name, out stream))
			{
				stream.Position = 0;
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Remove a chunk if it exists
	/// </summary>
	public bool RemoveChunk(string name)
	{
		return m_ChunkTable.Remove(name);
	}

	/// <summary>
	/// Empty internal table (Does not clean up any dangling streams)
	/// </summary>
	public void Clear()
	{
		m_ChunkTable.Clear();
	}
}
*/