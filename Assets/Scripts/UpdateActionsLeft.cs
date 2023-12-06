using CharlesEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateActionsLeft : MonoBehaviour
{
    public IntVariable actionsLeftVar;
    public BoolVariable shelterVar;
    public BoolVariable weaponsVar;

    public void UpdateActions()
    {
        GameObject.FindGameObjectWithTag("ActionsLeft").GetComponent<TMP_Text>().text = "Actions left: " + actionsLeftVar.RuntimeValue;

        if (actionsLeftVar.RuntimeValue <= 0)
        {
            GameObject.FindGameObjectWithTag("WoodButton").transform.GetChild(0).GetComponent<Button>().interactable = false;
            GameObject.FindGameObjectWithTag("WaterButton").transform.GetChild(0).GetComponent<Button>().interactable = false;
            GameObject.FindGameObjectWithTag("FoodButton").transform.GetChild(0).GetComponent<Button>().interactable = false;
            if (!shelterVar.RuntimeValue) GameObject.FindGameObjectWithTag("ShelterButton").transform.GetChild(0).GetComponent<Button>().interactable = false;
            if (!weaponsVar.RuntimeValue) GameObject.FindGameObjectWithTag("WeaponsButton").transform.GetChild(0).GetComponent<Button>().interactable = false;
        }
    }
}