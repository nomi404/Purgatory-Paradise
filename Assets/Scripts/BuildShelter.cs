using CharlesEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildShelter : MonoBehaviour
{
    public IntVariable woodVar;
    public BoolVariable weaponsVar;

    public void Build()
    {
        GameObject.FindGameObjectWithTag("Shelter").transform.GetChild(0).gameObject.SetActive(true);
        GameObject.FindGameObjectWithTag("ShelterButton").SetActive(false);
        GameObject.FindGameObjectWithTag("WoodCount").GetComponent<TMP_Text>().text = woodVar.RuntimeValue + "x";

        if (!weaponsVar.RuntimeValue && woodVar.RuntimeValue < 10) GameObject.FindGameObjectWithTag("WeaponsButton").transform.GetChild(0).GetComponent<Button>().interactable = false;
    }
}