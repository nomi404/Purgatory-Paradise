using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class ImportProcessor : AssetPostprocessor
{
 
	void OnPreprocessTexture()
	{
		Object asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D));
		if (asset)
			return; //set default values only for new textures;

		var scaler = 1;
		if (assetPath.Contains("@"))
		{
			var ind = assetPath.IndexOf("@") + 1;
			if (!int.TryParse( assetPath[ind].ToString(), out scaler))
			{
				scaler = 1;
			}
		}
		if (assetPath.Contains("cursor") || assetPath.Contains("_XIP"))
		{
			return;
		}
		TextureImporter importer = assetImporter as TextureImporter;
		//set your default settings to the importer here
		importer.alphaIsTransparency = true;
		importer.crunchedCompression = false;
		importer.mipmapEnabled = false;
		
		if( assetPath.Contains("_c1"))
		{
			importer.textureCompression = TextureImporterCompression.Compressed;
			importer.compressionQuality = 80;
			importer.crunchedCompression = true;
		}else if (assetPath.Contains("_c2"))
		{
			importer.textureCompression = TextureImporterCompression.Compressed;
			importer.crunchedCompression = false;
		}
		else
		{
			importer.textureCompression = TextureImporterCompression.Uncompressed;
		}

		importer.textureType = TextureImporterType.Sprite;
		importer.filterMode = FilterMode.Bilinear;
		importer.isReadable = false;
		if ( Math.Abs(importer.spritePixelsPerUnit - 100) < 1f || scaler > 1 )
		{
			importer.spritePixelsPerUnit = scaler;
		}

		Debug.Log(assetPath);
	}
 
}