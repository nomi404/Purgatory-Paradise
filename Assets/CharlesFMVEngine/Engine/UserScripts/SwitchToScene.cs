using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CharlesEngine
{
	[AddComponentMenu("CE Scripts/Switch To Scene")]
	public class SwitchToScene : CEScript
	{
		[Tooltip("Careful! breaks if you rename the scene")]
		public SceneField Destination;
		[Tooltip("Leave blank to start from the first node as usual")]
		public string DialogNodeID;

		public bool FadeToBlack;

		[HideInInspector] public UnityEvent OnSwitch;
		[ContextMenu("Run")]
		public override void Run()
		{
			var startWithNodeIdVariable = (StringVariable) Globals.Persistence.VariableManager.GetVariableByName("SwitchToDialogNode");
			startWithNodeIdVariable.RuntimeValue = DialogNodeID;
			Globals.GameManager.LoadScene(Destination, FadeToBlack);
			OnSwitch.Invoke();
		}

		public void SetDestination(string dest)
		{
			Destination.SceneName = dest;
		}
		
		public void SetDestination(Scene dest)
		{
			Destination.SceneName = dest.name;
		}
	}
}