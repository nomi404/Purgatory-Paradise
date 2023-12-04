using UnityEngine;
using UnityEngine.Events;

namespace CharlesEngine
{
    [AddComponentMenu("CE Scripts/If Else Script")]
    public class IfElseScript : ConditionalScript
    {
        public UnityEvent Else;
        public override void Run()
        {
            if (Condition.Eval())
            {
                Event?.Invoke();
            }
            else
            {
                Else?.Invoke();
            }
        }
    }
}