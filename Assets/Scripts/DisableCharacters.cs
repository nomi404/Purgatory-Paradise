using CharlesEngine;
using UnityEngine;

public class DisableCharacters : MonoBehaviour
{
    public IntVariable survivor1Var;
    public IntVariable survivor2Var;
    public IntVariable survivor3Var;
    public IntVariable survivor4Var;

    public void DisableSelectors()
    {
        if (survivor1Var.RuntimeValue <= 0) GameObject.FindGameObjectWithTag("Survivor1Selector").SetActive(false);
        if (survivor2Var.RuntimeValue <= 0) GameObject.FindGameObjectWithTag("Survivor2Selector").SetActive(false);
        if (survivor3Var.RuntimeValue <= 0) GameObject.FindGameObjectWithTag("Survivor3Selector").SetActive(false);
        if (survivor4Var.RuntimeValue <= 0) GameObject.FindGameObjectWithTag("Survivor4Selector").SetActive(false);
    }
}