using System;
using UnityEngine;

namespace CharlesEngine
{
    public abstract class Variable<T> : BaseVariable, IComparable where T : IComparable
    {
        //-- Serialization //
        [Serializable]
        private class VariableData<TV> : VariableData
        {
            public TV Value;

            public override string ToString()
            {
                return Value.ToString();
            }

            public override VariableData Clone()
            {
                return new VariableData<TV> {Value = Value};
            }
        }
        private VariableData<T> _data;
        //----------
	
        public event Action<T> OnVariableChange;
	
        public T DefaultValue;

        protected T _runtimeVal;
        public virtual T RuntimeValue
        {
            get { return _runtimeVal; }
            set
            {
                if (value == null && _runtimeVal == null)
                {
                    return;
                }
                if (value == null || value.CompareTo(_runtimeVal) != 0)
                {
                    _runtimeVal = value;
                    OnVariableChange?.Invoke(_runtimeVal);
                }
            }
        }
        [Header("Comment")]
        [Tooltip("This is purely for development")]
        [TextArea(5,19)]
        public string Comment;
	
        protected void OnEnable()
        {
            ResetValue();
        }

        public override void ResetValue()
        {
            RuntimeValue = DefaultValue;
        }

        // Serialization -------------//
        public override VariableData GetData()
        {
            if (_data == null)
            {
                _data = new VariableData<T>();
            }
            _data.Value = _runtimeVal;
            return _data;
        }

        public override void LoadFromData(VariableData data)
        {
            _data = data as VariableData<T>;
            if (_data == null)
            {
                Debug.LogError("Persistence error on variable "+Guid);
                return;
            }
            RuntimeValue = _data.Value;
        }
		
        public int CompareTo(object obj)
        {
            if (obj != null && (obj is Variable<T>))
            {
                return RuntimeValue.CompareTo( ((Variable<T>) obj).RuntimeValue );
            }
            return -1;
        }
    }
}