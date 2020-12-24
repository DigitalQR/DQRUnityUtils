using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace DQR.Types
{
	/// <summary>
	/// Attach this to anything which the element serializer can serialized
	/// </summary>
	public interface ISerializableElement
	{
		/// <summary>
		/// This will be checked during write serialization to avoid writing data if not required
		/// </summary>
		bool ShouldSerializationExport { get; }

		/// <summary>
		/// The internal key this component will be serialized under
		/// </summary>
		string SerializationKey { get; }

		/// <summary>
		/// The version number used to keep compatiblity between format changes
		/// </summary>
		int SerializationVersion { get; }

		/// <summary>
		/// Perform serialization for this element
		/// </summary>
		/// <param name="stream">The stream to read/write to/from</param>
		/// <param name="version">If reading will be the version number from input stream, if writing will be the save as SerializationVersion</param>
		void SerializeElement(SerializationStream stream, int version);
	}
}