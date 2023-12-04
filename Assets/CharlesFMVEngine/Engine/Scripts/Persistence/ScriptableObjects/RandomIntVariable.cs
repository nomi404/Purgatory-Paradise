using CharlesEngine;
using UnityEngine;

[CreateAssetMenu(fileName = "randIntVar",menuName ="Variables/RandomInt")]
public class RandomIntVariable : IntVariable
{
    public bool RollOnlyOnce;
    private int _firstRoll;
    private int _numRolls;
    public override int RuntimeValue
    {
        get
        {
            int result = Random.Range(MinValue,MaxValue);
            if (RollOnlyOnce && _numRolls == 0)
            {
                _firstRoll = result;
            }
            else if (RollOnlyOnce)
            {
                return _firstRoll;
            }
            _numRolls++;
            return result;
        }
        set
        {
            PersistanceType = PersistenceType.InMemory;
            base.RuntimeValue = value;
        }
    }
}
