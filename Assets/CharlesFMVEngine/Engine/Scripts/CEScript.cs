using UnityEngine;

namespace CharlesEngine
{
    public abstract class CEScript : MonoBehaviour, IValidable
    {
        protected virtual void Start()
        {
#if UNITY_EDITOR
            Validate();
#endif
        }

        public abstract void Run();

        public virtual bool Validate()
        {
            return true;
        }
    }
}
