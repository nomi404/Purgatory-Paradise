using UnityEngine;
using UnityEngine.Events;

namespace CharlesEngine
{
        [AddComponentMenu("CE Scripts/StringSet Observer")]
        public class StringSetObserver : CEScript //should be merged with VariableObserver
        {
            public StringSetVariable Variable;
            public IntVariable CountVar;
            public UnityEvent OnChangeEvent;
            

            protected override void Start()
            {
                base.Start();
                Variable.OnItemAdded += OnVarChanged;
            }

            private void OnVarChanged(string s)
            {
                CountVar.RuntimeValue = Variable.Count();
                OnChangeEvent?.Invoke();
            }

            public override void Run()
            {
                //nothing
            }
        }
}