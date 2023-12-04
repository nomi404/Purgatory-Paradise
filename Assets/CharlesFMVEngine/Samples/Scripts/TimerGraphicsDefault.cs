namespace CharlesEngineSamples
{
    using CharlesEngine;
    using UnityEngine;

    [RequireComponent(typeof(SpriteRenderer))]
    public class TimerGraphicsDefault : TimerGraphics
    {
        private SpriteRenderer _renderer;
        protected override void Start()
        {
            _renderer = GetComponentInChildren<SpriteRenderer>();
            _renderer.enabled = false;
            base.Start();
        }
        protected override void UpdateTime(float time, float totalTime)
        {
            _renderer.enabled = true;
            var percent = time / totalTime;
            transform.localScale = new Vector3(1-percent, transform.localScale.y, 1f);
            if (percent <= 0)
            {
                _renderer.enabled = false;
            }
        }
    }
}