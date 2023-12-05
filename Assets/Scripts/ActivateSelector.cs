using UnityEngine;

public class ActivateSelector : MonoBehaviour
{
    public void ActivateByName(string name)
    {
        if (name == "WoodSelector") { GameObject.FindGameObjectWithTag("WoodSelector").transform.GetChild(0).gameObject.SetActive(true); return; }
        if (name == "WaterSelector") { GameObject.FindGameObjectWithTag("WaterSelector").transform.GetChild(0).gameObject.SetActive(true); return; }
        if (name == "FoodSelector") { GameObject.FindGameObjectWithTag("FoodSelector").transform.GetChild(0).gameObject.SetActive(true); return; }
    }
}