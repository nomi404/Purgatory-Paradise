using CharlesEngine;
using UnityEngine;

[CreateAssetMenu(fileName = "intZeroBuildTest", menuName = "BuildTest/IntZeroBuildTest", order = 2)]
public class IntVariableZeroBuildTest : BuildTest
{
    public IntVariable[] VariablesThatMustBeZero;
    
    public override bool RunTest()
    {
        foreach (var svar in VariablesThatMustBeZero)
        {
            if (svar.DefaultValue != 0)
            {
                Debug.LogError("Default value of :" + svar.name + " is not zero!");
                return false;
            }
        }

        return true;
    }
}