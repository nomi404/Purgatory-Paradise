using CharlesEngine;
using UnityEngine;

public class RandomBoolVariable : BoolVariable
{
    public override bool RuntimeValue
    {
        get => Random.value > 0.5;
        set
        {
            PersistanceType = PersistenceType.InMemory; // Never persisted
            base.RuntimeValue = value;
        }
    }
}
