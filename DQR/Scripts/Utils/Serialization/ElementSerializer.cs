using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class ElementSerializer
{
	/// <summary>
	/// Wrapper function which will safely serialize a block of elements using SerializationChunk
	/// Assumes elements is a static collection to read data from or write data to
	/// </summary>
	public static void Serialize(SerializationStream stream, IEnumerable<ISerializableElement> elements)
	{
		AssertNonUniqueKeys(elements);

		if (stream.Mode == SerializationMode.Read)
		{
			byte[] data = null;
			stream.SerializeBlock(ref data);
			MemoryStream dataStream = new MemoryStream(data);

			using (var groupStream = new GroupSerializationStream(dataStream, stream.Mode))
			{
				foreach (var elem in elements)
				{
					if (groupStream.OpenSerializationStream(elem.SerializationKey, out SerializationStream elemStream))
					{
						int version = elem.SerializationVersion;
						elemStream.Serialize(ref version);
						elem.SerializeElement(elemStream, version);
					}
				}
			}
		}
		else
		{
			MemoryStream dataStream = new MemoryStream();

			using (var groupStream = new GroupSerializationStream(dataStream, stream.Mode))
			{
				foreach (var elem in elements)
				{
					if (elem.ShouldSerializationExport && groupStream.OpenSerializationStream(elem.SerializationKey, out SerializationStream elemStream))
					{
						int version = elem.SerializationVersion;
						elemStream.Serialize(ref version);
						elem.SerializeElement(elemStream, version);
					}
				}
			}

			byte[] data = dataStream.ToArray();
			stream.SerializeBlock(ref data);
		}
	}

	public static void AssertNonUniqueKeys(IEnumerable<ISerializableElement> elements)
	{
#if UNITY_ASSERTIONS
		var groupedElems = elements.GroupBy((e) => e.SerializationKey);
		foreach (var group in groupedElems.Where((g) => g.Count() != 1))
		{
			Debug.AssertFormat(false, "Found duplicate serialization keys in the same group for '{0}'", group.Key);
		}
#endif
	}
}
