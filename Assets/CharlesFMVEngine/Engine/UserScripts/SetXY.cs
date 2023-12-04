using CharlesEngine;
using UnityEngine;

namespace CharlesEngine
{

    [AddComponentMenu("CE Scripts/Set XY")]
    public class SetXY : CEScript
    {
        public Transform Target;

        public float X;
        public float Y;

        public override void Run()
        {
            Target.localPosition = new Vector3(X, Y, Target.localPosition.z);
        }
    }
}