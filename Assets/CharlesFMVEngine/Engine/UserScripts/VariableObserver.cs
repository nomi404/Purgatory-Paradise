using UnityEngine;
using UnityEngine.Events;

namespace CharlesEngine
{
	[AddComponentMenu("CE Scripts/Variable Observer")]
	public class VariableObserver : CEScript
	{
		public IntVariable Variable;

		public UnityEvent OnChangeEvent;

		protected override void Start()
		{
            base.Start();
			Variable.OnVariableChange += OnVarChanged;
		}

		private void OnVarChanged(int newval)
		{
			OnChangeEvent?.Invoke();
		}

		public override void Run()
		{
			//nothing
		}
	}
}
