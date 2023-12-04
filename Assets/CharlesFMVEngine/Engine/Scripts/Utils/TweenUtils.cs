using System;
using System.Collections;
using UnityEngine;

namespace CharlesEngine
{
    public static class TweenUtils
    {
        public static void DoFade(this MonoBehaviour o, SpriteRenderer r, float time, Action OnComplete = null)
        {
            o.StartCoroutine(DoFadeOverlay(r, time, 0, OnComplete));
        }
        
        public static void DoFade(this MonoBehaviour o, SpriteRenderer r, float time, float targetA = 0, Action OnComplete = null)
        {
            o.StartCoroutine(DoFadeOverlay(r, time, targetA, OnComplete));
        }
        
        private static IEnumerator DoFadeOverlay(SpriteRenderer r, float time, float targetA = 0, Action OnComplete = null)
        {
            var startA = r.color.a;
            var steps = time / Time.smoothDeltaTime;
            var astep = (startA-targetA) / steps;
            for (int i = 0; i < steps; i++)
            {
                r.color = new Color(r.color.r, r.color.g, r.color.b, startA-i*astep);
                yield return null;
            }
            r.color = new Color(r.color.r, r.color.g, r.color.b, targetA);
            if (OnComplete != null)
            {
                OnComplete.Invoke();
            }
        }
        
        
        public static void DoFade(this MonoBehaviour o, AlphaGroup r, float time, float targetA = 0, Action OnComplete = null)
        {
            o.StartCoroutine(DoFadeOverlay(r, time, targetA, OnComplete));
        }
        
        private static IEnumerator DoFadeOverlay(AlphaGroup r, float time, float targetA = 0, Action OnComplete = null)
        {
            var startA = r.Alpha;
            var steps = time / Time.smoothDeltaTime;
            var astep = (startA-targetA) / steps;
            for (int i = 0; i < steps; i++)
            {
                r.SetA(startA-i*astep);
                yield return null;
            }
            r.SetA(targetA);
            if (OnComplete != null)
            {
                OnComplete.Invoke();
            }
        }
    }
}