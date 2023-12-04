using UnityEngine;

// Utility screen object for debugging
namespace CharlesEngine
{
	public class DebugSO : ScriptableObject {

		public void Print(string msg)
		{
			Debug.Log(msg);
		}

		public void PrintTime()
		{
			Debug.Log(Time.realtimeSinceStartup);
		}

		public void Break()
		{
			Debug.Log("Break");
			Debug.Break();
		}
		
		public void PrintVariable(BaseVariable bv)
		{
			Debug.Log(bv);
		}
	}
}
