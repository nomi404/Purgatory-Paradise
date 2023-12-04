using UnityEngine;
using UnityEngine.Events;

namespace CharlesEngine
{
	[AddComponentMenu("CE Scripts/Conditional Script")]
	public class ConditionalScript : CEScript
	{
		public Condition Condition;
		public UnityEvent Event;
		public override void Run()
		{
			if (Condition.Eval())
			{
				Event?.Invoke();
			}
		}
		
		public bool RunBool()
		{
			if (Condition.Eval())
			{
				Event?.Invoke();
				return true;
			}

			return false;
		}
	}
}
