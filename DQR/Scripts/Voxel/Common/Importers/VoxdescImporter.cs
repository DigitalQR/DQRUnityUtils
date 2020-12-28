#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using DQR.Voxel;
using DQR.Voxel.Common;
using DQR.Models;
using DQR.Types;
using UnityEditor;


[UnityEditor.AssetImporters.ScriptedImporter(1, "voxdesc")]
public class VoxdescImporter : UnityEditor.AssetImporters.ScriptedImporter
{
	public enum LayoutOrientation
	{
		Vertical,
		Horizontal
	}

	[SerializeField]
	private LayoutOrientation m_ImportFrameOrientation = LayoutOrientation.Vertical;

	[SerializeField]
	private CommonVoxelImportSettings m_ImportSettings = new CommonVoxelImportSettings();
	

	public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
	{
		// Attempt to generate models for each frame
		string expectedImage = Path.Combine(Path.GetDirectoryName(ctx.assetPath), Path.GetFileNameWithoutExtension(ctx.assetPath));
		Texture2D sourceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(expectedImage);

		if (sourceTexture == null)
		{
			ctx.LogImportError($"Failed to load associated texture at '{expectedImage}'");
			return;
		}

		ctx.DependsOnSourceAsset(expectedImage);
		IVoxelVolume textureVolume = new Texture2DVoxelVolume(sourceTexture);

		// Layers will be stacked next to each other progressing
		int layerCount = m_ImportSettings.GetTotalChannelCountRounded(4) / 4;

		Vector3Int cellSize, frameStep, channelStep;

		if (m_ImportFrameOrientation == LayoutOrientation.Vertical)
		{
			cellSize = new Vector3Int();
			cellSize.x = sourceTexture.width / layerCount;
			cellSize.y = sourceTexture.height / m_ImportSettings.FrameCount;

			frameStep = new Vector3Int(0, cellSize.y, 0);
			channelStep = new Vector3Int(cellSize.x, 0, 0);
		}
		else
		{
			cellSize = new Vector3Int();
			cellSize.x = sourceTexture.width / m_ImportSettings.FrameCount;
			cellSize.y = sourceTexture.height / layerCount;
			
			frameStep = new Vector3Int(cellSize.x, 0, 0);
			channelStep = new Vector3Int(0, cellSize.y, 0);
		}

		CommonVoxelImporter importer = new CommonVoxelImporter(textureVolume, m_ImportSettings, cellSize, frameStep, channelStep);
		MeshSheet meshSheet = importer.GenerateMeshSheet();
		
		ctx.AddObjectToAsset("Meshes", meshSheet);
		for (int i = 0; i < meshSheet.FrameCount; ++i)
		{
			Mesh mesh = meshSheet.GetMeshAtFrame(i);
			mesh.name = Path.GetFileNameWithoutExtension(ctx.assetPath) + "_MeshFrame_" + i;
			ctx.AddObjectToAsset("MeshFrame_" + i, mesh);
		}

		ctx.SetMainObject(meshSheet);
	}
}
#endif