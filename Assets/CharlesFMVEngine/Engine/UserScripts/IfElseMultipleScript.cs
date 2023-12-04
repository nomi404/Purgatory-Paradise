using UnityEngine;

namespace CharlesEngine
{
    [AddComponentMenu("CE Scripts/If Else Multiple Script")]
    public class IfElseMultipleScript : CEScript
    {
        public ConditionalScript[] Scripts;
        public CEScript ElseScript;
        public override void Run()
        {
            for (var i = 0; i < Scripts.Length; i++)
            {
                if (Scripts[i] != null && Scripts[i].RunBool())
                {
                    return;
                }
            }

            ElseScript?.Run();
        }
    }
}