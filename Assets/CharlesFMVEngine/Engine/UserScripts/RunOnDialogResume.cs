using UnityEngine;

namespace CharlesEngine
{
	public class RunOnDialogResume : MonoBehaviour
	{
		public CEScript ScriptToRun;	
		void Awake()
		{
			var startWithNodeIdVariable = (StringVariable) Globals.Persistence.VariableManager.GetVariableByName("SwitchToDialogNode");
			if (!startWithNodeIdVariable.Empty())
			{
				FindObjectOfType<CEScene>().OnScreenShow.AddListener(OnScreenStart);
			}
		}

		private void OnScreenStart()
		{
			ScriptToRun.Run();
		}
	}
}