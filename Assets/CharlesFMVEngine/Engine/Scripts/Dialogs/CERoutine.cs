using System.Collections;
using UnityEngine;

namespace CharlesEngine
{
    public abstract class CERoutine : CEScript
    {
        public override void Run()
        {
            
        }

        public virtual IEnumerator RunRoutine()
        {
            yield return null;
        }
    }
}