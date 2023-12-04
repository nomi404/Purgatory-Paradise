using UnityEngine;

namespace CharlesEngine
{
    public class DialogObserver : MonoBehaviour
    {
        public virtual void NodeVisited(Node node)
        {
           // Debug.Log("visited:" + node);
           // Override this method to extend Dialog functionality
        }
    }
}