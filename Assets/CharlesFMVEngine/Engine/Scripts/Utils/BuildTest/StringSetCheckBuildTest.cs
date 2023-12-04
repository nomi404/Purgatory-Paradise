using CharlesEngine;
using UnityEngine;

[CreateAssetMenu(fileName = "stringSetBuildTest", menuName = "BuildTest/StringSetBuildTest", order = 2)]
public class StringSetCheckBuildTest : BuildTest
{
    public StringSetVariable[] Sets;
    public int MustHaveThisManyElements;
    
    public override bool RunTest()
    {
        foreach (var svar in Sets)
        {
            if (svar.DefaultValue.List.Count > MustHaveThisManyElements)
            {
                Debug.LogError("Set:" + svar.name + " has "+svar.DefaultValue.List.Count+" values, should have:"+MustHaveThisManyElements);
                return false;
            }
        }

        return true;
    }
}
