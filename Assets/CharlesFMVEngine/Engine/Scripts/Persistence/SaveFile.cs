using System;

namespace CharlesEngine
{
	[Serializable]
	public class SaveFile
	{
		public VariableRevision ProfileVariables;
		public string LastScene;
		public float PlayedSeconds;
	}
}
