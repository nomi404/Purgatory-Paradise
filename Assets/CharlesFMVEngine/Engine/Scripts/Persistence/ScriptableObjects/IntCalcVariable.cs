using CharlesEngine;
using UnityEngine;

[CreateAssetMenu(fileName = "intCalcVar",menuName ="Variables/IntegerCalculation")]
public class IntCalcVariable : IntVariable
{
    public IntVariable[] Sum;
    public new IntVariable[] Subtract;
    
    public override int RuntimeValue
    {
        get
        {
            int result = 0;
            foreach (var s in Sum)
            {
                result += s.RuntimeValue;
            }
            
            foreach (var s in Subtract)
            {
                result -= s.RuntimeValue;
            }
            return result;
        }
        set
        {
            PersistanceType = PersistenceType.InMemory;
            base.RuntimeValue = value;
        }
    }
}
