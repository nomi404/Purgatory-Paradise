using UnityEngine;
using UnityEngine.Events;

namespace CharlesEngine
{
    public class PauseMenuBase : MonoBehaviour
    {
        [HideInInspector]
        public UnityEvent OnPauseCancel;
        public virtual void Init()
        {

        }
    }
}
