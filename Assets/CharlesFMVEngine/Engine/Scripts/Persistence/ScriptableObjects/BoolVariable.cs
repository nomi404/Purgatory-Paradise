using UnityEngine;

namespace CharlesEngine
{
    [CreateAssetMenu(fileName = "boolVar",menuName ="Variables/Bool")]
    public class BoolVariable : Variable<bool>
    {

        public void SetTrue()
        {
            RuntimeValue = true;
        }

        public void SetFalse()
        {
            RuntimeValue = false;
        }

        public void FlipValue()
        {
            RuntimeValue = !RuntimeValue;
        }

        public void SetFromAnotherVariable(BoolVariable boolVariable)
        {
            RuntimeValue = boolVariable.RuntimeValue;
        }
        
        public override string ToString()
        {
            return name + " value:" + RuntimeValue;
        }
    }
}
