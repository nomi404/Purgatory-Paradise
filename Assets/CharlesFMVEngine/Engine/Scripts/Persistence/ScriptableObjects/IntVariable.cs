using UnityEngine;

namespace CharlesEngine
{
	[CreateAssetMenu(fileName = "intVar",menuName ="Variables/Integer")]
	public class IntVariable : Variable<int>
	{
		public int MaxValue = int.MaxValue;
		public int MinValue = 0;
	
		public override int RuntimeValue
		{
			get { return _runtimeVal; }
			set
			{
				if (value < MinValue) value = MinValue;
				if (value > MaxValue) value = MaxValue;
				base.RuntimeValue = value;
			}
		}
	
		public void IncrementValue()
		{
			RuntimeValue++;
		}
	
		public void DecrementValue()
		{
			RuntimeValue--;
		}

		public void Add(int val)
		{
			RuntimeValue += val;
		}
		
		public void Subtract(int val)
		{
			RuntimeValue -= val;
		}
		
		public void SetMax(int a)
		{
			RuntimeValue = Mathf.Max(RuntimeValue, a);
		}

        public void SetMin(int a)
        {
            RuntimeValue = Mathf.Min(RuntimeValue, a);
        }

        public void MultiplyBy(int val)
        {
	        RuntimeValue *= val;
        }
        
        public void SetFromAnotherVariable(IntVariable intVariable)
        {
	        RuntimeValue = intVariable.RuntimeValue;
        }
        
        public void AddAnotherVariable(IntVariable intVariable)
        {
	        RuntimeValue += intVariable.RuntimeValue;
        }
    }
}