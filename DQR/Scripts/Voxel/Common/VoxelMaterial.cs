using DQR.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Voxel.Common
{
	[System.Serializable]
	public class VoxelMaterial
	{
		public VoxelMaterialBasic Properties;

		public SerializableDictionary<VoxelFace, int> DressingLookup = new SerializableDictionary<VoxelFace, int>();

		public List<WeightedCollection<GameObject>> DressingSettings = new List<WeightedCollection<GameObject>>();

		public VoxelMaterial()
		{
		}

		public static bool ShouldGenerateFaceBetween(VoxelMaterial a, VoxelMaterial b)
		{
			if (a != null && b != null)
				return a.Properties.IsOpaque != b.Properties.IsOpaque;

			// One of or both are null if here, so assume one is empty
			else if (a != null || b != null)
				return true;
			
			return false;
		}

		public static VoxelMaterial Lerp(VoxelMaterial a, VoxelMaterial b, float t)
		{
			VoxelMaterial output = new VoxelMaterial();
			output.Properties = VoxelMaterialBasic.Lerp(a.Properties, b.Properties, t);

			if (t <= 0.0f)
			{
				output.DressingLookup = new SerializableDictionary<VoxelFace, int>(a.DressingLookup);
				output.DressingSettings = new List<WeightedCollection<GameObject>>(a.DressingSettings);
			}
			else if (t >= 1.0f)
			{
				output.DressingLookup = new SerializableDictionary<VoxelFace, int>(b.DressingLookup);
				output.DressingSettings = new List<WeightedCollection<GameObject>>(b.DressingSettings);
			}
			else
			{
				output.DressingLookup = new SerializableDictionary<VoxelFace, int>();
				output.DressingSettings = new List<WeightedCollection<GameObject>>();


				foreach (VoxelFace face in VoxelFaceHelpers.ToFaceCollection(VoxelFaces.All))
				{
					if (!a.DressingLookup.TryGetValue(face, out int aIdx))
						aIdx = -1;
					if (!b.DressingLookup.TryGetValue(face, out int bIdx))
						bIdx = -1;

					WeightedCollection<GameObject> settings = null;

					if (aIdx != -1 && bIdx != -1)
					{
						// Mix settings 
						settings = new WeightedCollection<GameObject>();
						settings.Insert(t, a.DressingSettings[aIdx]);
						settings.Insert(1.0f - t, b.DressingSettings[aIdx]);
					}
					else if (aIdx != -1)
					{
						settings = a.DressingSettings[aIdx];
					}
					else if (bIdx != -1)
					{
						settings = b.DressingSettings[bIdx];
					}

					if (settings != null)
					{
						int index = output.DressingSettings.Count;
						output.DressingSettings.Add(settings);
						output.DressingLookup.Add(face, index);
					}
				}
			}
			
			return output;
		}

	}
}
