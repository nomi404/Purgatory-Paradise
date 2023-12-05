using CharlesEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MakeWeapons : MonoBehaviour
{
    public IntVariable woodVar;
    public BoolVariable shelterVar;

    public void Make()
    {
        GameObject.FindGameObjectWithTag("Weapons").transform.GetChild(0).gameObject.SetActive(true);
        GameObject.FindGameObjectWithTag("WeaponsButton").SetActive(false);
        GameObject.FindGameObjectWithTag("WoodCount").GetComponent<TMP_Text>().text = woodVar.RuntimeValue + "x";

        if (!shelterVar.RuntimeValue && woodVar.RuntimeValue < 10) GameObject.FindGameObjectWithTag("ShelterButton").transform.GetChild(0).GetComponent<Button>().interactable = false;
    }
}