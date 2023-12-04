using CharlesEngine;
using UnityEngine;

namespace Uween
{
    public class TweenAG : TweenVec1
    {
        public static TweenAG Add(GameObject g, float duration)
        {
            return Add<TweenAG>(g, duration);
        }

        public static TweenAG Add(GameObject g, float duration, float to)
        {
            return Add<TweenAG>(g, duration, to);
        }

        private AlphaGroup G;

        protected AlphaGroup GetGraphic()
        {
            if (G == null)
            {
                G = GetComponent<AlphaGroup>();
            }

            return G;
        }

        protected override float Value
        {
            get { return GetGraphic().Alpha; }
            set
            {
                var g = GetGraphic();
                g.SetA(value);
            }
        }
    }
}