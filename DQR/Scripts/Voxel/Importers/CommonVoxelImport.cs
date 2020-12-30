using DQR.Models;
using DQR.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Voxel.Common
{
	//[System.Serializable]
	//public class CommonVoxelImportSettings
	//{
	//	public int FrameCount = 1;
	//	public bool FlipFrameDirection = true;
	//	public TextureLayoutFormat[] TextureLayout = new TextureLayoutFormat[] { TextureLayoutFormat.Albedo, TextureLayoutFormat.Alpha, TextureLayoutFormat.Tint, TextureLayoutFormat.Specular, TextureLayoutFormat.Roughness };
	//
	//	[Header("Animation")]
	//	public float FrameDurationScale = 1.0f;
	//	public float DefaultFrameDuration = 1.0f;
	//	public SerializableDictionary<int, float> FrameDurationOverrides = new SerializableDictionary<int, float>();
	//	public MeshSheet.PlaybackMode PlaybackMode = MeshSheet.PlaybackMode.PlayOnce;
	//
	//	[Header("Output")]
	//	public VoxelModelGenerationSettings ModelSettings = new VoxelModelGenerationSettings();
	//
	//	public enum TextureLayoutFormat
	//	{
	//		None,
	//		Skip1,
	//		Skip2,
	//		Skip3,
	//		Skip4,
	//		Albedo,
	//		Alpha,
	//		Tint,
	//		CustomUV2,
	//		CustomUV4,
	//		Specular,
	//		Roughness
	//	}
	//
	//	public static int GetChannelCount(TextureLayoutFormat format)
	//	{
	//		switch (format)
	//		{
	//			case TextureLayoutFormat.Skip4:
	//			case TextureLayoutFormat.Tint:
	//			case TextureLayoutFormat.CustomUV4:
	//				return 4;
	//
	//			case TextureLayoutFormat.Skip3:
	//			case TextureLayoutFormat.Albedo:
	//				return 3;
	//
	//			case TextureLayoutFormat.Skip2:
	//			case TextureLayoutFormat.CustomUV2:
	//				return 2;
	//
	//			case TextureLayoutFormat.Skip1:
	//			case TextureLayoutFormat.Alpha:
	//			case TextureLayoutFormat.Specular:
	//			case TextureLayoutFormat.Roughness:
	//				return 1;
	//
	//			default:
	//				return 0;
	//		}
	//	}
	//
	//	public int GetTotalChannelCount()
	//	{
	//		int layers = 0;
	//		foreach (var layer in TextureLayout)
	//			layers += GetChannelCount(layer);
	//		return layers;
	//	}
	//
	//	public int GetTotalChannelCountRounded(int roundMultiple)
	//	{
	//		int count = GetTotalChannelCount();
	//
	//		if (count % roundMultiple == 0)
	//			return count;
	//
	//		return (count / roundMultiple + 1) * roundMultiple;
	//	}
	//}
	//
	//internal class CommonVoxelImporter : IVoxelMaterialResolver
	//{
	//	private IVoxelVolume m_SourceVolume;
	//	private CommonVoxelImportSettings m_Settings;
	//	private Vector3Int m_CellSize;
	//	private Vector3Int m_ChannelStep;
	//	private Vector3Int m_FrameStep;
	//
	//	public CommonVoxelImporter(IVoxelVolume source, CommonVoxelImportSettings settings, Vector3Int cellSize, Vector3Int frameStep, Vector3Int channelStep)
	//	{
	//		m_SourceVolume = source;
	//		m_Settings = settings;
	//
	//		m_CellSize = cellSize;
	//		m_ChannelStep = channelStep;
	//		m_FrameStep = frameStep;
	//	}
	//
	//	private MeshSheetFrame GenerateFrame(int frame)
	//	{
	//		Vector3Int offset = m_FrameStep * frame;
	//		IVoxelVolume frameVolume = new VoxelVolumeSubview(m_SourceVolume, offset, m_CellSize);
	//
	//		VoxelModelGenerationRequest request = VoxelModelGenerationRequest.NewModelRequestSync(frameVolume, this, m_Settings.ModelSettings);
	//
	//		MeshSheetFrame outFrame = new MeshSheetFrame();
	//		outFrame.m_Mesh = request.GetMeshOutput();
	//
	//		int f = frame;
	//		if (m_Settings.FlipFrameDirection)
	//			f = m_Settings.FrameCount - frame - 1;
	//
	//		if (m_Settings.FrameDurationOverrides.TryGetValue(f, out float duration))
	//			outFrame.m_Duration = duration;
	//		else
	//			outFrame.m_Duration = m_Settings.DefaultFrameDuration;
	//
	//		outFrame.m_Duration *= Mathf.Max(0.001f, m_Settings.FrameDurationScale);
	//		return outFrame;
	//	}
	//
	//	public MeshSheet GenerateMeshSheet()
	//	{
	//		MeshSheetFrame[] frames = new MeshSheetFrame[m_Settings.FrameCount];
	//
	//		for (int i = 0; i < frames.Length; ++i)
	//		{
	//			int f = i;
	//			if (m_Settings.FlipFrameDirection)
	//				f = frames.Length - i - 1;
	//
	//			frames[f] = GenerateFrame(i);
	//		}
	//
	//		MeshSheet sheet = MeshSheet.CreateNew(frames);
	//		sheet.PlayMode = m_Settings.PlaybackMode;
	//		return sheet;
	//	}
	//
	//	private float[] ReadAllChannels(Vector3Int coord)
	//	{
	//		int channelCount = m_Settings.GetTotalChannelCount();
	//		int roundedChannelCount = m_Settings.GetTotalChannelCountRounded(4);
	//		float[] output = new float[roundedChannelCount];
	//
	//		for (int i = 0; i < channelCount;)
	//		{
	//			VoxelCell cell = m_SourceVolume.GetVoxelCell(coord + m_ChannelStep * (i / 4));
	//			output[i++] = cell.m_uint8_0 / (float)byte.MaxValue;
	//			output[i++] = cell.m_uint8_1 / (float)byte.MaxValue;
	//			output[i++] = cell.m_uint8_2 / (float)byte.MaxValue;
	//			output[i++] = cell.m_uint8_3 / (float)byte.MaxValue;
	//		}
	//
	//		return output;
	//	}
	//
	//	private float GetAlpha(Vector3Int coord)
	//	{
	//		float[] channels = ReadAllChannels(coord);
	//
	//		int i = 0;
	//		foreach (var layout in m_Settings.TextureLayout)
	//		{
	//			int channelSize = CommonVoxelImportSettings.GetChannelCount(layout);
	//
	//			if (layout == CommonVoxelImportSettings.TextureLayoutFormat.Alpha)
	//				return channels[i];
	//
	//			i += channelSize;
	//		}
	//
	//		return 0;
	//	}
	//
	//	public bool ShouldConsiderForModel(Vector3Int coord, VoxelCell cell)
	//	{
	//		return cell != VoxelCell.Invalid && GetAlpha(coord) >= 0.1f;
	//	}
	//
	//	public bool ShouldAddFace(Vector3Int fromCoord, VoxelCell fromCell, Vector3Int toCoord, VoxelCell toCell)
	//	{
	//		return toCell == VoxelCell.Invalid || GetAlpha(fromCoord) != GetAlpha(toCoord);
	//	}
	//
	//	public VoxelVertexOutput ResolveVoxelVertex(VoxelVertexInput input, VoxelModelGenerationSettings settings)
	//	{
	//		float[] channels = ReadAllChannels(input.Coord);
	//		VoxelMaterialProperties material = new VoxelMaterialProperties();
	//		List<Vector4> customUVs = new List<Vector4>();
	//		
	//		int c = 0;
	//		foreach (var layout in m_Settings.TextureLayout)
	//		{
	//			switch (layout)
	//			{
	//				case CommonVoxelImportSettings.TextureLayoutFormat.Tint:
	//					material.Tint = new Vector4(
	//						channels[c + 0],
	//						channels[c + 1],
	//						channels[c + 2],
	//						channels[c + 3]
	//					);
	//					break;
	//
	//				case CommonVoxelImportSettings.TextureLayoutFormat.Albedo:
	//					material.Colour.r = channels[c + 0];
	//					material.Colour.g = channels[c + 1];
	//					material.Colour.b = channels[c + 2];
	//					break;
	//
	//				case CommonVoxelImportSettings.TextureLayoutFormat.Alpha:
	//					material.Colour.a = channels[c];
	//					break;
	//					
	//				case CommonVoxelImportSettings.TextureLayoutFormat.CustomUV2:
	//					customUVs.Add(new Vector4(channels[c + 0], channels[c + 1], 0, 0));
	//					break;
	//
	//				case CommonVoxelImportSettings.TextureLayoutFormat.CustomUV4:
	//					customUVs.Add(new Vector4(channels[c + 0], channels[c + 1], channels[c + 2], channels[c + 3]));
	//					break;
	//
	//				case CommonVoxelImportSettings.TextureLayoutFormat.Specular:
	//					material.Specular = channels[c];
	//					break;
	//
	//				case CommonVoxelImportSettings.TextureLayoutFormat.Roughness:
	//					material.Roughness = channels[c];
	//					break;
	//			}
	//
	//			int channelSize = CommonVoxelImportSettings.GetChannelCount(layout);
	//			c += channelSize;
	//		}
	//
	//		material.CustomUVs = customUVs.ToArray();
	//
	//		VoxelVertexOutput output = new VoxelVertexOutput();
	//		output.SetDefaults(input, settings);
	//
	//		material.ApplyToVertex(input, ref output);
	//		return output;
	//	}
	//}
}
