using CharlesEngine;
using UnityEngine;

public class SetActiveButtons : MonoBehaviour
{
    private GameObject woodButton;
    private GameObject waterButton;
    private GameObject foodButton;
    private GameObject shelterButton;
    private GameObject weaponsButton;

    public BoolVariable shelterVar;
    public BoolVariable weaponsVar;

    private void Start()
    {
        woodButton = GameObject.FindGameObjectWithTag("WoodButton");
        waterButton = GameObject.FindGameObjectWithTag("WaterButton");
        foodButton = GameObject.FindGameObjectWithTag("FoodButton");
        shelterButton = GameObject.FindGameObjectWithTag("ShelterButton");
        weaponsButton = GameObject.FindGameObjectWithTag("WeaponsButton");
    }

    public void SetActive(bool active)
    {
        woodButton.transform.GetChild(0).gameObject.SetActive(active);
        waterButton.transform.GetChild(0).gameObject.SetActive(active);
        foodButton.transform.GetChild(0).gameObject.SetActive(active);
        if (!shelterVar.RuntimeValue) shelterButton.transform.GetChild(0).gameObject.SetActive(active);
        if (!weaponsVar.RuntimeValue) weaponsButton.transform.GetChild(0).gameObject.SetActive(active);
    }
}