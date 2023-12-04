using System.Collections.Generic;
using UnityEngine;

namespace CharlesEngine
{
	public class SoundPool : MonoBehaviour
	{

		private readonly int _prewarm = 18;

		private readonly List<SoundPlayer> _players = new List<SoundPlayer>();
		private readonly List<SoundPlayer> _pausedPlayers = new List<SoundPlayer>();

		private int _current = 0;

		private const string SoundVolumeKey = "volume"; 
	
		void Start ()
		{
			var orig = transform.GetChild(0);

			_players.Add(orig.GetComponent<SoundPlayer>());
			while (_players.Count < _prewarm)
			{
				var newsource = Instantiate(orig, transform);
				_players.Add(newsource.GetComponent<SoundPlayer>());
			}
			
			// global volume setting
			if (PlayerPrefs.HasKey(SoundVolumeKey))
			{
				var vol = PlayerPrefs.GetFloat(SoundVolumeKey);
				AudioListener.volume = GetLogSoundVolume(vol);
				CEVideoPlayer.GlobalVolumeLevel = GetLogSoundVolume(vol);
			}
		}

		public void SetGlobalVolume(float f)
		{
			f = Mathf.Clamp01(f);
			PlayerPrefs.SetFloat(SoundVolumeKey, f);
			AudioListener.volume = GetLogSoundVolume(f);
			CEVideoPlayer.GlobalVolumeLevel = GetLogSoundVolume(f);
		}

		private float GetLogSoundVolume(float f)
		{
			return (Mathf.Pow(40, f) - 1f) / 39f;
		}

		public float GetGlobalVolume()
		{
			if (PlayerPrefs.HasKey(SoundVolumeKey))
			{
				var vol = PlayerPrefs.GetFloat(SoundVolumeKey);
				return vol;
			}
			return 1f;
		}
	
		/*
	 * Always discard the return value after clip ends, SoundPlayers are pooled!
	 */
		public SoundPlayer Play(AudioClip clip, string ID, float delayTime, bool loop = false)
		{
			var src = _players[_current];
			while (src.IsPlaying)
			{
				_current = (_current + 1) % _players.Count;
				src = _players[_current];
			}
			src.Play(clip, ID, delayTime, loop);
			_current = (_current + 1) % _players.Count;
			return src;
		}

		public void Stop(string ID)
		{
			foreach (var p in _players)
			{
				if (p.SoundID == ID)
				{
					p.StopSound();
					return;
				}
			}
			Debug.LogWarning("Sound ID not found:"+ID);
		}

		public void StopAll()
		{
			foreach (var p in _players)
			{
				p.StopSound();
				p.OnSoundEnd.RemoveAllListeners();
			}
		}

		public void PauseAll()
		{
			foreach (var p in _players)
			{
				if (p.IsPlaying)
				{
					p.Pause();
					_pausedPlayers.Add(p);
				}
			}
		}

		public void Resume()
		{
			foreach (var p in _pausedPlayers)
			{
				p.Resume();
			}
			_pausedPlayers.Clear();
		}
	}
}
