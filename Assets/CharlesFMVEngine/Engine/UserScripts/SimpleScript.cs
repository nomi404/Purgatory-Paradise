using UnityEngine;
using UnityEngine.Events;

namespace CharlesEngine
{
	[AddComponentMenu("CE Scripts/SimpleScript")]
	public class SimpleScript : CEScript
	{
		public UnityEvent Event;

		public override void Run()
		{
			Event.Invoke();
		}
	}
}
