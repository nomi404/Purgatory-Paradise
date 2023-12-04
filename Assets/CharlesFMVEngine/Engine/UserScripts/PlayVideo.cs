using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using Uween;

namespace CharlesEngine
{
	[AddComponentMenu("CE Scripts/PlayVideo")]
	public class PlayVideo : CEScript, IInputHandler
	{
		public VideoClip Clip;
	
		public UnityEvent OnVideoEnd;

		public bool FadeOutAtEnd;

		public bool SpaceSkippable;

		public bool StayOnLastFrame;
	
		private CEVideoPlayer _player;
		private const float FadeDuration = 1.0f;
		private bool _skipped;
	
		public override void Run()
		{
			_player = Globals.Videos.PlayVideo(Clip);
			_player.AddActionOnLastFrame(VideoEnd);
			var subtitlesData = Globals.Subtitles.LoadSubtitles(Clip.name);
			if (subtitlesData != null)
			{
				Globals.Subtitles.ShowSubtitles(_player.Scheduler, subtitlesData);
			}
			if (FadeOutAtEnd)
			{
				_player.Scheduler.AddListener(FadeOutVideo, _player.Duration - FadeDuration - 0.05f);
			}
		
			// Register On Movie Time events
			var timeEvents = GetComponents<ScheduleTimeEvent>();
			foreach (var trig in timeEvents)
			{
				trig.RegisterOn(_player.Scheduler);
			}
			if (SpaceSkippable)
			{
				Globals.GameManager.RegisterInputHandler(this, 1);
			}

			if (StayOnLastFrame)
			{
				_player.SkipNextFade();
			}

			_skipped = false;
		}

		private void VideoEnd(TimedScheduler scheduler)
		{
			if (!StayOnLastFrame)
			{
				_player.SmoothStop();
			}


			OnVideoEnd.Invoke();
		
			if (SpaceSkippable)
			{
				Globals.GameManager.RemoveInputHandler(this);
			}
		}

		public void Prepare()
		{
			Globals.Videos.PrepareVideo(Clip);
		}

		public void StopVideo()
		{
			if (!_player)
			{
				Debug.LogWarning("StopVideo called, but video was not playing");
				return;
			}
		
			// Clear the changed values due to Fading
			_player.SetAlpha(1);
			var audioSource = _player.GetComponent<AudioSource>();
			// reset audio
			audioSource.volume = 1;
			TweenV.KillAll(_player.gameObject);
			//audioSource.DOKill(); // clear tween
		
			// stop Video
			_player.ResetPlayer(); // kills video and sounds immedietely

			OnVideoEnd.Invoke();
			Globals.Subtitles.Hide();
		
			_player = null;
		}

		private void FadeOutVideo(TimedScheduler t)
		{
			FadeOutVideo();
		}
	
		public void FadeOutVideo()
		{
			if (_player == null)
			{
				Debug.LogWarning("FadeOutVideo called, but video was not playing");
				return;
			}

			_player.FadeToBlack(FadeDuration);
			//DOTween.To(_player.GetAlpha, _player.SetAlpha, 0.0f, FadeDuration);
			TweenV.Add(_player.gameObject, FadeDuration, 0).Then(StopVideo);
			//_player.GetComponent<AudioSource>().DOFade(0, FadeDuration).OnComplete(StopVideo);;
		}

	/*	public void FadeInVideo()
		{
			Run();

			_player.SetAlpha(0);
			DOTween.To(_player.GetAlpha, _player.SetAlpha, 1f, FadeDuration);
		
			var audioSource = _player.GetComponent<AudioSource>();
			audioSource.volume = 0;
			audioSource.DOFade(1, FadeDuration);
		}*/

		public bool HandleInput()
		{
			if (Globals.Input.GetKeyUp(InputAction.SkipVideo))
			{
				if (!_skipped && _player != null && _player.IsPlaying && _player.Time > 1)
				{
					Globals.Videos.Skip();
					_skipped = true;
					return true;
				}
			}
			return false;
		}
	}
}
