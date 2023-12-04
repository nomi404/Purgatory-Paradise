using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.Events;

namespace CharlesEngine
{
    
    // Used for timing things instead of Invoke, because it stops the timers when the game is paused
    public class CEUtils : MonoBehaviour
    {
        private class DelayItem<T> : DelayItem
        {
            protected readonly Action<T> ActionT;
            protected T Arg;
            public DelayItem(Action<T> a, float d, T arg) : base(null, d)
            {
                ActionT = a;
                Delay = d;
                Arg = arg;
            }
            public override void Invoke()
            {
                ActionT.Invoke(Arg);   
            }
        }
        
        private class DelayItem
        {
            private static int _autoCount;
            protected readonly Action Action;
            public float Delay;
            public readonly int Id;
            public DelayItem(Action a, float d)
            {
                Action = a;
                Delay = d;
                Id = _autoCount++;
            }

            public virtual void Invoke()
            {
                Action.Invoke();   
            }
        }

        private List<DelayItem> _delayItems = new List<DelayItem>(6);
        private List<DelayItem> _delayItemsSwap = new List<DelayItem>(6);
        
        public int Delay(Action action, float delay)
        {
            var delayItem = new DelayItem(action, delay);
            _delayItems.Add(delayItem);
            return delayItem.Id;
        }
        
        public int Delay<T>(Action<T> action, float delay, T arg)
        {
            var delayItem = new DelayItem<T>(action, delay, arg);
            _delayItems.Add(delayItem);
            return delayItem.Id;
        }

        public void CancelDelay(int id)
        {
            _delayItemsSwap.Clear();
            for (var i = 0; i < _delayItems.Count; i++)
            {
                if (_delayItems[i].Id != id)
                {
                    _delayItemsSwap.Add(_delayItems[i]);
                }
            }
            // swap lists 
            var tmp = _delayItems;
            _delayItems = _delayItemsSwap;
            _delayItemsSwap = tmp;
        }

        public float GetRemainingTime(int id)
        {
            for (var i = 0; i < _delayItems.Count; i++)
            {
                if (_delayItems[i].Id == id)
                {
                    return _delayItems[i].Delay;
                }
            }

            return 0;
        }

        private void Update()
        {
            if (Globals.Pause.IsPaused || _delayItems.Count == 0)
            {
                return;
            }

            bool hasRemoves = false;
            for (var i = 0; i < _delayItems.Count; i++)
            {
                _delayItems[i].Delay -= Time.deltaTime;
                if (_delayItems[i].Delay <= 0)
                {
                    hasRemoves = true;
                    _delayItems[i].Invoke();
                }
            }

            if (hasRemoves)
            {
                // copy only valid items
                _delayItemsSwap.Clear();
                for (var i = 0; i < _delayItems.Count; i++)
                {
                    if (_delayItems[i].Delay > 0)
                    {
                        _delayItemsSwap.Add(_delayItems[i]);
                    }
                }
                // swap lists 
                var tmp = _delayItems;
                _delayItems = _delayItemsSwap;
                _delayItemsSwap = tmp;
            }
        }
        
        public static IEnumerator WaitUntilEvent(UnityEvent unityEvent) {
            var trigger = false;
            Action action = () => trigger = true;
            unityEvent.AddListener(action.Invoke);
            yield return new WaitUntil(()=>trigger);
            unityEvent.RemoveListener(action.Invoke);
        }
    }
}