using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using System;

namespace CharlesEngine
{
	[RequireComponent(typeof(Collider2D))]
	[AddComponentMenu("CE Toolbox/Event Listener")]
	public class EventListener : MonoBehaviour {

		public UnityEvent OnMouseEnterEvent = new UnityEvent();
		public UnityEvent OnMouseExitEvent = new UnityEvent();
		public UnityEvent OnMouseClick = new UnityEvent();
	
		private void OnMouseEnter()
		{
			OnMouseEnterEvent.Invoke();
		}
	
		private void OnMouseExit()
		{
			OnMouseExitEvent.Invoke();
		}
		
		private void OnMouseUpAsButton()
		{
			OnMouseClick.Invoke();
		}
		
#if UNITY_EDITOR
		[ContextMenu("Switch First and Last in OnClick")]
		public void SwitchFirstAndLastOnClick()
		{
			var persistentEventCount = OnMouseClick.GetPersistentEventCount();
			if (persistentEventCount <= 1) return;
			var targetref = OnMouseClick.GetPersistentTarget(0);
			var methodname = OnMouseClick.GetPersistentMethodName(0);
			UnityEventTools.RemovePersistentListener(OnMouseClick,0);
			
			var targetInfo = UnityEvent.GetValidMethodInfo(targetref ,methodname, new Type[0] );
			UnityAction mdelegate = Delegate.CreateDelegate(typeof(UnityAction), targetref, targetInfo) as UnityAction;
			UnityEventTools.AddPersistentListener(OnMouseClick, mdelegate);
		}
#endif
	}
}
