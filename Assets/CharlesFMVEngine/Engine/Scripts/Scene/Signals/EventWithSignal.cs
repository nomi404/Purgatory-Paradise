using System;
using UnityEngine;
using UnityEngine.Events;

namespace CharlesEngine
{
    [AddComponentMenu("CE Toolbox/Event with signal")]
    public class EventWithSignal : CEScript, IValidable
    {
        [Header("FIRST DO THIS")]
        public UnityEvent BeforeEvent;
        
        [Header("WAIT FOR A SIGNAL")]
        public CESignal Signal;
        
        [Header("THEN DO THIS")]
        public UnityEvent AfterEvent;

        private bool runRunning = false;

        private bool signalCaught = false;

        public override bool Validate()
        {
            if (Signal == null)
            {
                Debug.LogError("NO SIGNAL DEFINED ON SCRIPT: " + gameObject.name, gameObject);
                return false;
            }
            return true;
        }

        public override void Run()
        {
            if (runRunning)
            {
                Debug.LogError("TRYING TO RUN THIS SCRIPT RECURSIVELY: " + gameObject.name, gameObject);
                return;
            }

            Signal.Event.AddListener(OnAfterEvent);
            runRunning = true;
            try
            {
                BeforeEvent.Invoke();
                if (signalCaught)
                {
                    signalCaught = false;
                    AfterEvent.Invoke();
                }
            }
            finally
            {
                runRunning = false;
            }            
        }

        public void OnAfterEvent()
        {
            if (runRunning)
            {
                signalCaught = true;
                return;
            }
            Signal.Event.RemoveListener(OnAfterEvent);
            AfterEvent.Invoke();
        }
    }
}