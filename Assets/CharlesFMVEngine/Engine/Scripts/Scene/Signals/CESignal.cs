using System;
using UnityEngine;
using UnityEngine.Events;

namespace CharlesEngine
{
    [CreateAssetMenu(fileName = "ce_signal",menuName ="CESignal/Signal")]
    public class CESignal : ScriptableObject
    {
        [NonSerialized] public UnityEvent Event;
        
        [Header("Comment")]
        [Tooltip("This is purely for development")]
        [TextArea(5,19)]
        public string Comment;

        private void OnEnable()
        {
            if (Event == null)
            {
                Event = new UnityEvent();
            }
        }

        public void Fire()
        {
            Event.Invoke();
        }
    }
}