using UnityEngine;

namespace CharlesEngine
{
	public class CustomCondition : MonoBehaviour
	{
		private void Start()
		{
		
		}

		public virtual bool Eval()
		{
			return true;
		}
	}
}