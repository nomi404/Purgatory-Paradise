using UnityEngine;
using UnityEngine.Events;

namespace CharlesEngine
{
	[AddComponentMenu("CE Scripts/Timer")]
	public class Timer : CEScript
	{
		[Range(0,240)]
		public float Time;
		public UnityEvent OnTimer;
		protected int _delayId = -1;
		public int DelayId => _delayId;

		public override void Run()
		{
			if (_delayId >= 0)
			{
				Globals.Utils.CancelDelay(_delayId); // cancel previous invoke
			}
			_delayId = Globals.Utils.Delay(TimerOver, Time);
		}

		private void TimerOver()
		{
			OnTimer?.Invoke();
			_delayId = -1;
		}

		public void Cancel()
		{
			if (_delayId >= 0)
			{
				Globals.Utils.CancelDelay(_delayId); // cancel previous invoke
				_delayId = -1;
			}
		}
		
		public void Pause()
		{
			if (_delayId >= 0)
			{
				Time = Globals.Utils.GetRemainingTime(_delayId);
				Globals.Utils.CancelDelay(_delayId);
				_delayId = -1;
			}
		}

		public bool IsActive()
		{
			return _delayId != -1;
		}
	}
}
