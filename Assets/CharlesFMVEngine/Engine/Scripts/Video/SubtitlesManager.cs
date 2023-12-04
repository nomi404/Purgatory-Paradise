using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;

namespace CharlesEngine
{
	public class SubtitlesManager : MonoBehaviour
	{
		[SerializeField] protected TextMeshPro Text;
	
		private SubtitlesData _data;
		private SubtitleItem _currentItem;

		public bool UserEnabled => _userEnabled;

		private bool _userEnabled;

		private AssetBundle _bundle;

		private const string SubtitlePrefsKey = "subtitles-on";
	
		private void Awake()
		{
		//	Instance = this;
			Text.gameObject.SetActive(false);
		}

		public void Init()
		{
			_userEnabled = true;
			if (string.IsNullOrEmpty(Globals.Lang))
			{
				Globals.Lang = "en";
			} 
			if (PlayerPrefs.HasKey(SubtitlePrefsKey))
			{
				_userEnabled = PlayerPrefs.GetInt(SubtitlePrefsKey) == 1;
			}
			string filePath = Application.streamingAssetsPath + "/AssetBundles/subtitles_" + Globals.Lang;
			
#if UNITY_EDITOR
			//Load AssetBundle, used for testing the asset bundle system only
			/*_bundle = AssetBundle.LoadFromFile(filePath);
			if (_bundle == null)
			{
				Debug.LogError("Subtitles bundle not found:"+filePath);
			}*/
#else
			StartCoroutine(LoadBundle(filePath));
#endif
		}

		public void UnloadBundles()
		{
			_bundle = null;
			AssetBundle.UnloadAllAssetBundles(true);
		}
		
		public IEnumerator ReloadLangBundles()
		{
#if UNITY_EDITOR
			yield break;
#endif
			AssetBundle.UnloadAllAssetBundles(true);
			yield return new WaitForSeconds(0.3f);
			string filePath = Application.streamingAssetsPath + "/AssetBundles/subtitles_" + Globals.Lang;
			yield return LoadBundle(filePath);
		}

		private IEnumerator LoadBundle(string filePath)
		{
			var loadedAb = AssetBundle.GetAllLoadedAssetBundles();
			foreach (var bnd in loadedAb)
			{
				if (bnd.name == Path.GetFileNameWithoutExtension(filePath))
				{
					yield break;
				}
			}
			
			var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
			yield return assetBundleCreateRequest;
			_bundle = assetBundleCreateRequest.assetBundle;
			if (_bundle == null)
			{
				Debug.LogWarning("Did not find subtitles.");
			}
		}
	
		public void ShowSubtitles(TimedScheduler player, SubtitlesData subtitles)
		{
			_data = subtitles;
			foreach (var it in subtitles.Items)
			{
				player.AddListener(RefreshSubtitles,it.Start);
				player.AddListener(RefreshSubtitles,it.End);
			}
		}
	
		public void Hide()
		{
			if (_currentItem != null)
			{
				Text.gameObject.SetActive(false);
				_currentItem = null;
			}
		}

		private void RefreshSubtitles(TimedScheduler player)
		{
			var t = player.CurrentTime;
			float closest = float.MaxValue;
			SubtitleItem closestItem = null;
			foreach (var it in _data.Items)
			{
				var delta = Mathf.Min(Mathf.Abs(it.Start - t), Mathf.Abs(it.End - t));
				if (delta < closest)
				{
					closest = delta;
					closestItem = it;
				}
			}
			if (closestItem == null) return;
			var startD = Mathf.Abs(closestItem.Start - t);
			var endD = Mathf.Abs(closestItem.End - t);
			if (startD > 1 && endD > 1)
			{
				Debug.Log("skipping subtitles");
			}
			if (startD < endD)
			{
				ShowItem(closestItem);
			}
			else
			{
				HideItem(closestItem);
			}
		}

		private void ShowItem(SubtitleItem item)
		{
			_currentItem = item;
			Text.gameObject.SetActive(_userEnabled);
			Text.text = item.Text;
		}

		private void HideItem(SubtitleItem item)
		{
			if (_currentItem == item)
			{
				Text.gameObject.SetActive(false);
				_currentItem = null;
			}
		}

		public void ToggleEnabled()
		{
			_userEnabled = !_userEnabled;
			PlayerPrefs.SetInt(SubtitlePrefsKey, _userEnabled ? 1 : 0);
			Text.gameObject.SetActive(_userEnabled && _currentItem != null);
		}
		
		public SubtitlesData LoadSubtitles(string clipName)
		{
			TextAsset asset = null;
#if UNITY_EDITOR
			var allFound = UnityEditor.AssetDatabase.FindAssets(clipName + "_" + Globals.Lang);
			if (allFound.Length > 0)
			{
				var fileGuid = allFound[0];
				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(fileGuid);
				asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
			}	
#else
			if( _bundle == null ) return null;
			asset = _bundle.LoadAsset<TextAsset>(clipName);
#endif
			if (asset == null)
			{
				Debug.LogWarning("Subtitles not found in bundle "+clipName + "_" + Globals.Lang);
			}

			return asset == null ? null : new SubtitlesData(asset.text, clipName);
		}
		
		
#if UNITY_EDITOR
		public static SubtitlesData LoadSubtitlesEditor(string clipName)
		{
			TextAsset asset = null;
			var allFound = UnityEditor.AssetDatabase.FindAssets(clipName + "_" + Globals.Lang);
			if (allFound.Length > 0)
			{
				var fileGuid = allFound[0];
				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(fileGuid);
				asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
			}

			return asset == null ? null : new SubtitlesData(asset.text, clipName);
		}
#endif

	}
}
