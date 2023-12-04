using System;
using UnityEngine;

namespace CharlesEngine
{
	[AddComponentMenu("CE Scripts/Multi Script")]
	public class MultiScript : CEScript {
		public override void Run()
		{
			var allScripts = GetComponents<CEScript>();
			foreach (var script in allScripts)
			{
				if (script == this) continue;
				try
				{
					script.Run();
				}
				catch (Exception e)
				{
					Debug.LogError("Error found when executing "+script.GetType().Name+"\n\nError:"+e.Message+"\n"+e.StackTrace+"\n", gameObject);
				}
			}
		}
	}
}
