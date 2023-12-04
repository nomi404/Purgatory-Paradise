using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace CharlesEngine
{
    public class TimerRoutine : CERoutine
    {
        [Range(0,150)]
        public float Seconds;

        private Timer _timer;
        public override IEnumerator RunRoutine()
        {
            if (_timer == null)
            {
                _timer = gameObject.AddComponent<Timer>();
                _timer.OnTimer = new UnityEvent();
            }
            else
            {
                _timer.Cancel();
            }
            _timer.Time = Seconds;
            _timer.Run();
            yield return CEUtils.WaitUntilEvent(_timer.OnTimer);
        }
    }
}