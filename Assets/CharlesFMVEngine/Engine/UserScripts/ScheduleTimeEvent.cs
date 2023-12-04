using UnityEngine;

namespace CharlesEngine
{
	[AddComponentMenu("CE Scripts/On Video Time")]
	public class ScheduleTimeEvent : SimpleScript
	{
		public float Time;

		public void RegisterOn(TimedScheduler scheduler)
		{
			scheduler.AddListener(OnTime, Time);
		}

		private void OnTime(TimedScheduler obj)
		{
			Run();
		}
	}
}
