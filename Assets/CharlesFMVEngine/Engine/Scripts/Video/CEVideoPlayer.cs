using System;
using UnityEngine;
using UnityEngine.Video;
using Uween;
using static CharlesEngine.CELogger;

namespace CharlesEngine
{
	[RequireComponent(typeof(VideoPlayer))]
	public class CEVideoPlayer : MonoBehaviour
	{		
		public static float GlobalVolumeLevel = 1f;
		
		public const float LastFrameTimeMark = 9999999f;
		private const float PreloadNextTimeMark = 7777777f;

		private const int ShowZ = -9; //the z (depth) of video currently playing 
		private const int HidingZ = -8; //the z (depth) of video being hidden

		public bool IsPreparing { get; private set; }
		public bool IsPrepared { get; private set; }
		public bool IsPlaying => _player != null && _player.isPlaying;
		public VideoClip Clip => _player?.clip;

		public float Duration => _player.frameCount / _player.frameRate;
		public double Time => _player.time; //seconds

		private VideoPlayer _player;
		private SpriteRenderer _spriteRenderer;

		private bool _playAfterPrepare;
		private bool _discardAfterPrepare;
		private bool _isPaused;
		
		public readonly TimedScheduler Scheduler = new TimedScheduler();

		private GameObject _blocker; // added a black background to block border pixels from shining through
		private SpriteRenderer _blockerRenderer;
		private int _layerNormal;
		private int _layerHidden;
		private bool _doNotFade;
		
		private Color TransparentColor = new Color(1,1,1,0);
		
		private void Awake()
		{
			_player = GetComponent<VideoPlayer>();
			_player.prepareCompleted += PlayerOnPrepareCompleted;
			_player.loopPointReached += OnPlayerEnd;
			_spriteRenderer = GetComponent<SpriteRenderer>();
			_blocker = transform.GetChild(0).gameObject;
			_blockerRenderer = _blocker.GetComponent<SpriteRenderer>();
		
			_layerNormal = LayerMask.NameToLayer("Default");
			_layerHidden = LayerMask.NameToLayer("NotRendered");
			SetLayer(_layerHidden);
		}
		public void InitSize(Vector2Int res)
		{
			_spriteRenderer = GetComponent<SpriteRenderer>();
			var t = new Texture2D(res.x, res.y);
			int numclrs = res.x * res.y;
			var data = new Color32[numclrs];
			for (int i = 0; i < numclrs; i++)
			{
				data[i] = new Color32(0, 0, 0, 0);
			}
			t.SetPixels32(data);
			_spriteRenderer.sprite = Sprite.Create(t, new Rect(0,0,res.x, res.y),Vector2.one/2f,1);
		}
		
		#region Schedulling
	
		public void AddActionOnLastFrame(Action<TimedScheduler> action)
		{
			Scheduler.AddListener(action, LastFrameTimeMark);
		}
	
		public void AddPreloadTimeAction(Action<TimedScheduler> action)
		{
			Scheduler.AddListener(action, PreloadNextTimeMark);
		}
	
		#endregion

		public void Prepare(VideoClip clip)
		{
			gameObject.SetActive(true);
			SetLayer(_layerHidden);
			IsPreparing = true;
			IsPrepared = false;
			_player.clip = clip;
			_player.audioOutputMode = VideoAudioOutputMode.Direct;
			_player.EnableAudioTrack(0, true);
			_player.Prepare();

			Scheduler.ClearListeners();
		}

		private void PlayerOnPrepareCompleted(VideoPlayer source)
		{
			Log("Finished prepare " + source?.clip?.name + " on:"+gameObject.name, VIDEOS);
			IsPreparing = false;
			IsPrepared = true;
			if (_playAfterPrepare)
			{
				_playAfterPrepare = false;
				PlayPlayerAndSound();
				TweenA.Add(gameObject, 0.3f, 1);
				SetZ(ShowZ);
				//_spriteRenderer.DOFade(1, 0.3f);
			}
			if (_discardAfterPrepare)
			{
				ResetPlayer();
			}
		}

		private void PlayPlayerAndSound()
		{
			Log("Actually playing "+_player.clip.name+" at:"+gameObject.name+" a:"+_spriteRenderer.color.a , VIDEOS);
			Globals.Videos.OnVideoStartPlaying.Invoke();
			SetLayer(_layerNormal);
			_player.Play();
			_player.SetDirectAudioVolume(0,GlobalVolumeLevel);
			TweenVidSound.KillAll(gameObject);
			SetZ(ShowZ);
		}
	
		public void PlayLooped(VideoClip clip)
		{
			Play(clip);
			_player.isLooping = true;
		}

		public void Pause()
		{
			if (_player.isPlaying)
			{
				_player.Pause();
				_isPaused = true;
			}
		}

		public void Resume()
		{
			if (_isPaused)
			{
				_isPaused = false;
				_player.Play();
			}
		}

		public void Play(VideoClip clip)
		{
			Scheduler.ResetTimeCallbacks();
			gameObject.SetActive(true);
			_player.isLooping = false;
			_isPaused = false;
			if (clip != _player.clip)
			{
				PlayWithoutPrepare(clip);
			}
			else if (IsPrepared)
			{
				PlayPlayerAndSound();
			}
			else if (IsPreparing)
			{
				Log("Play called in preparing process " + clip.name, VIDEOS);
				_playAfterPrepare = true;
			}
		}

		private void PlayWithoutPrepare(VideoClip clip)
		{
			if (_player.isPlaying)
			{
				Scheduler.ClearListeners();
				StopImmediate();
				_spriteRenderer.color = TransparentColor;
			}
			_player.gameObject.SetActive(true);
			Log("Playing unprepared video " + clip.name + " " + UnityEngine.Time.time, VIDEOS);
			_player.clip = clip;
			_spriteRenderer.color = TransparentColor;
			_player.audioOutputMode = VideoAudioOutputMode.Direct;
			_player.EnableAudioTrack(0, true);
			//StartCoroutine(TakeTimeToPrepare()); // Simulate long loading
			_player.Prepare();
			_player.SetDirectAudioVolume(0,GlobalVolumeLevel);
			IsPreparing = true;
			_playAfterPrepare = true;
			SetZ(0);
			CancelInvoke(nameof(ReallyHidePlayer));
		}

	/*	private IEnumerator TakeTimeToPrepare() // Simulates long loading
		{
			// TESTING ONLY	
			Log("BEFORE "+_player.clip+" "+Time.unscaledTime);
			yield return new WaitForSeconds(1f);
			Log("AFTER "+_player.clip+" "+Time.unscaledTime);
			_player.Prepare();
		}*/
		
		/// <summary>
		// This hides the video in a way that ensures a smooth transitions to the next video.
		// If there is no other video, use SmoothStop instead
		/// </summary>
		public void HidePlayer()
		{
			SetZ(HidingZ);
			TweenVidSound.Add(gameObject, 0.1f, 0);
			//_audioSource.DOFade(0, 0.1f);
			
			Log("HidePlayer " + _player.clip.name + " " + _player + " " + UnityEngine.Time.time, VIDEOS);
			if (DialogManager.IsActive && !_doNotFade)
			{
				TweenA.Add(gameObject, 0.3f, 0);
				//_spriteRenderer.DOFade(0, 0.3f);
				Log("HidePlayer fading", VIDEOS);
			}
			
			Invoke(nameof(ReallyHidePlayer), 0.5f); //avoid flickering
		}

		private void ReallyHidePlayer()
		{
			if (IsPreparing)
			{
				Log("Reset call ignored:" + gameObject.name, VIDEOS);
				return;
			}
			Log("ReallyHidePlayer " + _player?.clip?.name + " " + UnityEngine.Time.time, VIDEOS);
			SetAlpha(0);
			SetZ(0);
			ResetPlayer();
		}

		public void SetAlpha(float a)
		{
			var clr = _spriteRenderer.color;
			clr.a = a;
			_spriteRenderer.color = clr;
		
			clr = _blockerRenderer.color;
			clr.a = a;
			_blockerRenderer.color = clr;
		}

		public float GetAlpha()
		{
			return _spriteRenderer.color.a;
		}
		
		private void SetZ(float z)
		{
			transform.localPosition = new Vector3(0, 0, z);
		}

		private void SetLayer(int layer)
		{
			gameObject.layer = layer;
			_blocker.layer = layer;
		}

		public void SkipNextFade()
		{
			_doNotFade = true;
			Log("SkipNextFade " + _player, VIDEOS);
		}

		private void OnPlayerEnd(VideoPlayer source)
		{
			Log("OnPlayerEnd "+gameObject.name, VIDEOS);
			var currentTime = (float) _player.time;
			Scheduler.Update(currentTime);
			Scheduler.Mark(LastFrameTimeMark);
		}

		private void Update()
		{
			if (_player == null || _player.isPlaying == false) return;
			var currentTime = (float) _player.time;
		
			Scheduler.Update(currentTime);
			// LAST FRAME MARK
			bool lastFrame = _player.frame == (long) _player.frameCount - 3;
			if (lastFrame)
			{
				Scheduler.Mark(LastFrameTimeMark);
			}
		
			// PRELOAD MARK
			var preloadTime = Duration - 4f;
			if (preloadTime < 1)
			{
				preloadTime = Duration*0.8f;
			}
			if (currentTime >= preloadTime)
			{
				Scheduler.Mark(PreloadNextTimeMark);
			}
			
#if UNITY_EDITOR
			_player.playbackSpeed = UnityEngine.Time.timeScale;
#endif
		}
	
		public void Skip()
		{
			if (_player == null || _player.isPlaying == false) return;
			// 1. fire Preload next listener
			Scheduler.Mark(PreloadNextTimeMark);
			// 2. fire last frame listener
			Scheduler.Mark(LastFrameTimeMark);
			_player.Pause();
		}

		public void ResetPlayer()
		{
			Log("ResetPlayer "+gameObject.name, VIDEOS);
			if (IsPreparing)
			{
				Log("discard called while preparing "+gameObject.name, VIDEOS);
				_discardAfterPrepare = true;
				return;
			}
			Scheduler.ClearListeners();
			if (!_player) return;
			_player.isLooping = false;
			_player.clip = null;
			_playAfterPrepare = false;
			_discardAfterPrepare = false;
			_doNotFade = false;
			_player.SetDirectAudioVolume(0,GlobalVolumeLevel);
			SetAlpha(1);
			SetLayer(_layerHidden);
			gameObject.SetActive(false);
		}
	
		/// <summary>
		/// Hides the player, but fades the sound out smoothly
		/// </summary>
		public void SmoothStop()
		{
			if (_player == null) return;
			SetAlpha(0);
			TweenVidSound.Add(gameObject, 0.5f, 0).Then(ResetPlayer);
			Log("SmoothStop "+gameObject.name, VIDEOS);
		}

		public void StopImmediate()
		{
			_player.Stop();
		}
		
		public void SetVolume(float v)
		{
			_player?.SetDirectAudioVolume(0,v);
		}

		public void SkipToTime(float f)
		{
			_player.time = f;
		}

		public void FadeToBlack(float f)
		{
		//	_spriteRenderer.DOFade(0, f);
			TweenA.Add(gameObject, f, 0);
		}
	}
}