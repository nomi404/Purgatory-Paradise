using UnityEngine;

namespace CharlesEngine
{
	[AddComponentMenu("CE Toolbox/Comment")]
	public class Comment : MonoBehaviour
	{
		[Tooltip("This is purely for development")]
		[TextArea(5,19)]
		public string Message;
	}
}
