using System;
using UnityEngine;
using UnityEngine.Events;

namespace CharlesEngine
{
	[AddComponentMenu("CE Scripts/PlaySound")]
	public class PlaySound : CEScript
	{
		public AudioClip Clip;
		public string ClipID;
		[Range(0,10)]
		public float Delay;
	
		[Range(0,1)]
		public float Volume = 1f;

		[Tooltip("Check this if the engine should look for subtitles")]
		public bool HasSubtitles;

		public bool Loop;
	
		public UnityEvent OnSoundEnd;

		public bool IsSoundEffect;

		private SoundPlayer _player;

		public override void Run()
		{
			if (Clip == null)
			{
				Debug.LogError("PlaySound: no AudioClip specified!", gameObject);
				return;
			}
			if (_player != null && !HasSubtitles)
			{
				_player.Play(Clip, ClipID, Delay, Loop);
				return;
			}

			PlayInternal();
		
			// Register On Movie Time events
			var timeEvents = GetComponents<ScheduleTimeEvent>();
			foreach (var trig in timeEvents)
			{
				trig.RegisterOn(_player.Scheduler);
			}
		}

		private void PlayInternal()
		{
			_player = Globals.Sounds.Play(Clip, ClipID, Delay, Loop && !HasSubtitles); // loop here would not display subtitles on second play
			_player.SetVolume(Volume);
			if (IsSoundEffect)
			{
				_player = null;
				if (OnSoundEnd.GetPersistentEventCount() > 0)
				{
					Debug.LogError(gameObject+ " has OnSoundEnd listeners on a SoundEffect, they will not be called.", gameObject);
				}
				return;
			}
			_player.OnSoundEnd.AddListener(SoundEnd);
			if (HasSubtitles)
			{
				var subtitlesData = Globals.Subtitles.LoadSubtitles(Clip.name);
				if (subtitlesData != null)
				{
					Globals.Subtitles.ShowSubtitles(_player.Scheduler, subtitlesData);
				}
				else
				{
					Debug.LogWarning("Subtitles not found for:"+Clip.name);
				}
			}
		}
	
		private void SoundEnd()
		{
			_player.OnSoundEnd.RemoveListener(SoundEnd);
			OnSoundEnd.Invoke();
			if (Loop && HasSubtitles)
			{
				PlayInternal();
			}
		}

		public void StopSound()
		{
			if (!_player)
			{
				Debug.LogWarning("StopSound called, but sound was not playing");
				return;
			}
			_player.StopSound();
			_player.OnSoundEnd.RemoveListener(SoundEnd);
			if (HasSubtitles)
			{
				Globals.Subtitles.Hide();
			}
		}

		public void PauseSound()
		{
			if (!_player)
			{
				Debug.LogWarning("PauseSound called, but sound was not playing");
				return;
			}
			_player.Pause();
		}
		
		public void PauseSoundWithFadeOut()
		{
			if (!_player)
			{
				Debug.LogWarning("PauseSound called, but sound was not playing");
				return;
			}
			_player.PauseWithFadeOut();
		}

		public void FadeOutSound()
		{
			if( _player )
				_player.FadeOut(1);
		}
		
		public void FadeInSound()
		{
			Run();
			_player.SetVolume(0);
			_player.FadeIn(1);
		}
	
		public void ResumeSound()
		{
			if (!_player)
			{
				Debug.LogWarning("ResumeSound called, but sound was paused");
				return;
			}
			_player.Resume();
		}
	}
}
