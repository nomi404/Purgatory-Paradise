using UnityEngine;
using UnityEngine.Events;
using Uween;

namespace CharlesEngine
{
	[RequireComponent(typeof(AudioSource))]
	public class SoundPlayer : MonoBehaviour
	{
		// PUBLIC
		public UnityEvent OnSoundEnd;
		public readonly TimedScheduler Scheduler = new TimedScheduler();
		public string SoundID { get; private set; }
		public bool IsPlaying => _source.isPlaying;

		// PRIVATE
		private AudioSource _source;
		private int _finishedCount;
		private bool _isActive;
		private bool _isPaused;
		private int _scheduledId = -1;
		private void Awake()
		{
			_source = GetComponent<AudioSource>();
		}

		public void Play(AudioClip clip, string ID,  float delayTime = 0, bool loop = false)
		{
			SoundID = ID;
			_source.clip = clip;
			_source.loop = loop;
			_source.volume = 1f;
			_isActive = true;
			if (delayTime <= 0)
			{
				_scheduledId = -1;
				_source.Play();
			}
			else
			{
				_scheduledId = Globals.Utils.Delay(()=> Play(clip, ID, 0, loop), delayTime); // TODO remove closure
			}
		}
	
		public void StopSound()
		{
			if (_scheduledId >= 0)
			{
				Globals.Utils.CancelDelay(_scheduledId);
				_scheduledId = -1;
			}
			Scheduler.ClearListeners();
			if (_source.clip != null)
			{
				TweenV.Add(gameObject, 0.5f, 0).Then(ClearPlayer);
				//_source.DOFade(0, 0.5f).OnComplete(ClearPlayer);
			}
		}

		private void ClearPlayer()
		{
			_isActive = false;
			SoundID = null;
			_source.clip = null;
			_source.loop = false;
			_source.volume = 1f;
		}

		private void Update()
		{
			if (!_isActive || _isPaused ) return;
		
			Scheduler.Update(_source.time);
		
			if (!_source.isPlaying)
			{
				_finishedCount++; //we have to wait two frames, because of when the app loses focus
				if (_finishedCount >= 2)
				{
					SoundFinished();
				}
			}
			else
			{
				_finishedCount = 0;
			}
		}

		public void Pause()
		{
			_isPaused = true;
			_source.Pause();
		}
		
		public void PauseWithFadeOut()
		{
			TweenV.Add(gameObject, 1f, 0).Then(OnPauseFadeOutFinished);
			//_source.DOFade(0, 1).OnComplete( OnPauseFadeOutFinished );
		}

		private void OnPauseFadeOutFinished()
		{
			Pause();
			SetVolume(1);
		}

		public void Resume()
		{
			if (!_isPaused)
			{
				Debug.LogWarning("Resume called on Sound that was not paused.");
				return;
			}
			_isPaused = false;
			_source.UnPause();
		}

		private void SoundFinished()
		{
			_isActive = false;
			OnSoundEnd.Invoke();	
		}

		public void SetVolume(float volume)
		{
			_source.volume = volume;
		}

		public void FadeOut(float time)
		{
		//	_source.DOFade(0, time).OnComplete( SoundFinished );
			TweenV.Add(gameObject, time, 0f).Then(SoundFinished);
		}
		
		public void FadeIn(float time)
		{
		//	_source.DOFade(1, time).OnComplete( SoundFinished );
			TweenV.Add(gameObject, time, 1f).Then(SoundFinished);
		}
		
	}
}