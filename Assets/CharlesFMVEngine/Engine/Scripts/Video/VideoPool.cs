using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using Cursor = UnityEngine.Cursor;

namespace CharlesEngine
{
	public class VideoPool : MonoBehaviour
	{
		[NonSerialized] public readonly UnityEvent OnVideoStartPlaying = new UnityEvent();
		[NonSerialized] public readonly UnityEvent OnVideoEnd = new UnityEvent();
		[NonSerialized] public readonly UnityEvent OnVideoAlmostEnd = new UnityEvent();

		private readonly List<CEVideoPlayer> _players = new List<CEVideoPlayer>();

		private CEVideoPlayer _currentPlayer;
		private bool _prepareNotified;

		private GameObject _origPlayer;

		private readonly Dictionary<string,SubtitlesData> _subtitles = new Dictionary<string, SubtitlesData>();
		
		// Detecting mouse movements
		private float _timeMouseInactive;
		public bool HideMouseEnabled { get; set; }
		
		
		
		#region Subtitles

		public void LoadSubtitlesFor(VideoClip clip)
		{
			var subs = Globals.Subtitles.LoadSubtitles(clip.name);
			if (subs == null)
			{
				Debug.LogWarning("No subtitles found for:"+clip.name);
				return;
			}
			_subtitles[clip.name] = subs;
		}

		public void LoadSubtitlesFor(List<VideoClip> clips)
		{
			foreach (var clip in clips)
			{
				var subs = Globals.Subtitles.LoadSubtitles(clip.name);
				if (subs == null)
				{
					//	Debug.LogWarning("No subtitles found for:"+clip.name);
					continue;
				}
				_subtitles[clip.name] = subs;
			}
		}

		public void UnloadAllSubtitles()
		{
			_subtitles.Clear();
		}
		#endregion
	
		//===== UNITY CALLBACKS ============

		void Awake()
		{
			_origPlayer = transform.GetChild(0).gameObject;
			_players.Add(_origPlayer.GetComponent<CEVideoPlayer>());
			_players[0].InitSize(Globals.Settings.Resolution);
			AddPlayers(2);
			_origPlayer.SetActive(false);
			HideMouseEnabled = true;
		}

		void Update()
		{
			if (_currentPlayer == null || !_currentPlayer.IsPlaying)
			{
				Cursor.visible = true;
				return;
			}
			
			Vector2 direction = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
			if (direction.magnitude < 0.001f)
			{
				_timeMouseInactive += Time.deltaTime;
				if (_timeMouseInactive > 2f && HideMouseEnabled)
				{
					Cursor.visible = false;
				}
			}
			else
			{
				Cursor.visible = true;
				_timeMouseInactive = 0;
			}
		}
	
		// ========= PRIVATE ============

		private void AddPlayers(int count)
		{
			for (int i = 0; i < count; i++)
			{
				var p = Instantiate(_origPlayer, _origPlayer.transform.parent);
				p.name = "VideoPlayer" + i;
				p.transform.localPosition = new Vector3(0, 0, 0);
				var player = p.GetComponent<CEVideoPlayer>();
				_players.Add(player);
				player.InitSize(Globals.Settings.Resolution);
				p.SetActive(false);
			}
		}

		private void OnVideoAlmostEndCallback(TimedScheduler player)
		{
			OnVideoAlmostEnd.Invoke();
		}

		private void OnVideoEndCallback(TimedScheduler player)
		{
			OnVideoEnd.Invoke();
		}

		private CEVideoPlayer GetPlayerForClip(VideoClip clip)
		{
			if (clip == null)
			{
				Debug.LogError("Cannot play null video clip!");
				return null;
			}
			CEVideoPlayer player = GetAlreadyPrepared(clip);
			if (player == null)
			{
				CELogger.Log("No prepared video found for " + clip.name, CELogger.VIDEOS);
				player = GetFreePlayer();
			}
			return player;
		}

		private CEVideoPlayer GetFreePlayer()
		{
			foreach (var player in _players)
			{
				if (!player.gameObject.activeSelf || !player.IsPlaying)
				{
					return player;
				}
			}
			return null;
			//no free players
		}

		private CEVideoPlayer GetAlreadyPrepared(VideoClip clip)
		{
			foreach (var player in _players)
			{
				if (player.Clip == clip)
				{
					if (player.IsPlaying || player.IsPreparing || player.IsPrepared)
						return player;
				}
			}
			return null;
		}

		private void SetCurrentPlayer(CEVideoPlayer player)
		{
			if (_currentPlayer != null && player != _currentPlayer && _currentPlayer.gameObject.activeSelf)
			{
				_currentPlayer.HidePlayer();
			}
			_currentPlayer = player;
			_currentPlayer.AddActionOnLastFrame(OnVideoEndCallback);
			_currentPlayer.AddPreloadTimeAction(OnVideoAlmostEndCallback);
		}

		// ========= PUBLIC ============
		public void PrePool(int i)
		{
			AddPlayers(i);
		}
		
		public CEVideoPlayer PlayVideo(VideoClip clip)
		{		
			var player = GetPlayerForClip(clip);
			if (player == null)
			{
				throw new Exception("Too many videos playing at once, trying to play "+clip?.name);
			}
			CELogger.Log("Play video " + clip.name + " at:"+player.name, CELogger.VIDEOS);
			
			player.Play(clip);
			SubtitlesData subs;
			if (_subtitles.TryGetValue(clip.name,out subs))
			{
				Globals.Subtitles.ShowSubtitles(player.Scheduler, subs);
			}
			else
			{
				CELogger.Log("subs not found:" + clip.name, CELogger.VIDEOS);
			}
			SetCurrentPlayer(player);
			return player;
		}

		public CEVideoPlayer LoopVideo(VideoClip clip)
		{
			var player = GetPlayerForClip(clip);
			if (player == null)
			{
				throw new Exception("Too many videos playing at once, trying to play "+clip?.name);
			}
			CELogger.Log("Loop video " + clip?.name + " at:"+player.name, CELogger.VIDEOS);
			
			player.PlayLooped(clip);
			SetCurrentPlayer(player);
			return player;
		}
		
		/**
		* returns true if there is an available player to start preparing video 
		*/
		public bool PrepareVideo(VideoClip clip)
		{
			if (!clip) return false;
			if (GetAlreadyPrepared(clip) != null) return true;
			CEVideoPlayer playerNext = null;
			foreach (var player in _players)
			{
				if (!player.gameObject.activeSelf)
				{
					playerNext = player;
				}
			}

			if (playerNext)
			{
				CELogger.Log("Prepare video " + clip.name + " at:" + playerNext.name, CELogger.VIDEOS);
				playerNext.Prepare(clip);
				return true;
			}
			
			return false;
		}

		public void ReleasePreparedVideo(VideoClip clip)
		{
			var player = GetAlreadyPrepared(clip);
			//Debug.Log("Releasing prepared video "+player?.name);
			player?.ResetPlayer();
		}
	
		public void StopAll()
		{
			//Debug.Log("Stop All Videos");
			if (_currentPlayer != null && _currentPlayer.IsPlaying)
			{
				_currentPlayer.SmoothStop();
			}
		}
	
		public void PauseAll()
		{
			if (_currentPlayer != null && _currentPlayer.IsPlaying)
			{
				_currentPlayer.Pause();
			}
		}
	
		public void Resume()
		{
			if (_currentPlayer != null && !_currentPlayer.IsPlaying)
			{
				_currentPlayer.Resume();
			}
		}

		public void ResetAll()
		{
			if (_currentPlayer != null && _currentPlayer.IsPlaying)
			{
				_currentPlayer.StopImmediate();
			}
			foreach (var player in _players)
			{
				player.ResetPlayer();
			}
			foreach (var player in _players)
			{
				if (player.gameObject != _origPlayer)
				{
					Destroy(player.gameObject);
				}
			}
			_players.Clear();
			_players.Add(_origPlayer.GetComponent<CEVideoPlayer>());
			AddPlayers(2);
			_origPlayer.SetActive(false);
		}
	
		public void Skip()
		{
			if (IsPlaying())
			{
				_currentPlayer.Skip();
				Globals.Subtitles.Hide();
			}
		}

		public bool IsPlaying()
		{
			return _currentPlayer != null && _currentPlayer.IsPlaying && _currentPlayer.Time > 1;
		}
	}
}