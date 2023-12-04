using UnityEngine;

namespace CharlesEngine
{
	[AddComponentMenu("Dialog/Dialog Start Condition")]
	public class DialogStartCondition : MonoBehaviour
	{
		public Condition Condition;
		public Dialogues Dialog;
		public string Tree;
		public int TreeIndex;
		public string Node;

		private void Reset()
		{
			var dm =  FindObjectOfType<DialogManager>();
			Dialog = dm?.Dialogues;
		}
	}
}
