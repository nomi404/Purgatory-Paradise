using UnityEngine;

namespace CharlesEngine
{
    [RequireComponent(typeof(Timer))]
    public abstract class TimerGraphics : MonoBehaviour
    {
        private Timer Timer;
        protected virtual void Start()
        {
            Timer = GetComponent<Timer>();
        }

        protected abstract void UpdateTime(float time, float totalTime);

        void Update()
        {
            if (Timer.DelayId >= 0)
            {
                float remains = Globals.Utils.GetRemainingTime(Timer.DelayId);
                var totalTime = Timer.Time;
                UpdateTime(totalTime-remains,totalTime);
            }
        }
    }
}