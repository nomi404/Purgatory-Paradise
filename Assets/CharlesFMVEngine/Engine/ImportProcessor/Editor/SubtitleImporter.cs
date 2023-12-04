using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SubtitleImporter : AssetPostprocessor
{
	public static List<string> LoadedSubtitles = new List<string>();

	public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
		string[] movedFromAssetPaths)
	{
		bool anyAdded = false;
		foreach (string asset in importedAssets)
		{
			if (asset.EndsWith(".srt")) // copy this to a new asset with .txt extension, so it can be loaded as TextAsset
			{
				string filePath = asset.Substring(0, asset.Length - Path.GetFileName(asset).Length);
				string newFileName = filePath + Path.GetFileNameWithoutExtension(asset) + ".txt";

				if (!Directory.Exists(filePath))
				{
					Directory.CreateDirectory(filePath);
				}

				StreamReader reader = new StreamReader(asset);
				string fileData = reader.ReadToEnd();
				reader.Close();

				FileStream resourceFile = new FileStream(newFileName, FileMode.OpenOrCreate, FileAccess.Write);
				StreamWriter writer = new StreamWriter(resourceFile);
				writer.Write(fileData);
				writer.Close();
				resourceFile.Close();
				Debug.Log("Auto converting " + Path.GetFileName(asset) + " to txt");

				AssetDatabase.DeleteAsset(asset);
				
				LoadedSubtitles.Add(newFileName);
				anyAdded = true;
			}
			if (asset.EndsWith(".txt") && LoadedSubtitles.Contains(asset)) // Add the Subtitles label to the new txt asset
			{
				var newAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(asset);
				AssetDatabase.SetLabels(newAsset, new[] {"subtitles"});
			}
		}
		if (anyAdded)
		{
			AssetDatabase.Refresh();
		}
	}
}