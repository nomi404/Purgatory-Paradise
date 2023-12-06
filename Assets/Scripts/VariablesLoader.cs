using CharlesEngine;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VariablesLoader : MonoBehaviour
{
    public BoolVariable shelterVar;
    public BoolVariable weaponsVar;
    public IntVariable survivor1Var;
    public IntVariable survivor2Var;
    public IntVariable survivor3Var;
    public IntVariable survivor4Var;
    public IntVariable dayVar;
    public IntVariable woodVar;
    public IntVariable foodVar;
    public IntVariable waterVar;
    public IntVariable actionsLeftVar;
    public BoolVariable lookedForWoodVar;
    public BoolVariable lookedForWaterVar;
    public BoolVariable lookedForFoodVar;

    private void Start()
    {
        if (survivor1Var.RuntimeValue <= 0 && survivor2Var.RuntimeValue <= 0 && survivor3Var.RuntimeValue <= 0 && survivor4Var.RuntimeValue <= 0) SceneManager.LoadScene("GameOver");

        if (dayVar.RuntimeValue >= 8) SceneManager.LoadScene("Win");

        GameObject woodButton = GameObject.FindGameObjectWithTag("WoodButton");
        GameObject waterButton = GameObject.FindGameObjectWithTag("WaterButton");
        GameObject foodButton = GameObject.FindGameObjectWithTag("FoodButton");

        woodButton.transform.GetChild(0).GetComponent<Button>().interactable = !lookedForWoodVar.RuntimeValue;
        waterButton.transform.GetChild(0).GetComponent<Button>().interactable = !lookedForWaterVar.RuntimeValue;
        foodButton.transform.GetChild(0).GetComponent<Button>().interactable = !lookedForFoodVar.RuntimeValue;

        GameObject.FindGameObjectWithTag("Shelter").transform.GetChild(0).gameObject.SetActive(shelterVar.RuntimeValue);
        GameObject.FindGameObjectWithTag("Weapons").transform.GetChild(0).gameObject.SetActive(weaponsVar.RuntimeValue);

        GameObject shelterButton = GameObject.FindGameObjectWithTag("ShelterButton");
        shelterButton.transform.GetChild(0).gameObject.SetActive(!shelterVar.RuntimeValue);

        GameObject weaponsButton = GameObject.FindGameObjectWithTag("WeaponsButton");
        weaponsButton.transform.GetChild(0).gameObject.SetActive(!weaponsVar.RuntimeValue);

        if (!shelterVar.RuntimeValue && woodVar.RuntimeValue < 10) shelterButton.transform.GetChild(0).GetComponent<Button>().interactable = false;
        if (!weaponsVar.RuntimeValue && woodVar.RuntimeValue < 8) weaponsButton.transform.GetChild(0).GetComponent<Button>().interactable = false;

        if (actionsLeftVar.RuntimeValue <= 0)
        {
            woodButton.transform.GetChild(0).GetComponent<Button>().interactable = false;
            waterButton.transform.GetChild(0).GetComponent<Button>().interactable = false;
            foodButton.transform.GetChild(0).GetComponent<Button>().interactable = false;
            shelterButton.transform.GetChild(0).GetComponent<Button>().interactable = false;
            weaponsButton.transform.GetChild(0).GetComponent<Button>().interactable = false;
        }

        var gradient = new Gradient();

        var colors = new GradientColorKey[3];
        colors[2] = new GradientColorKey(Color.green, 1.0f);
        colors[1] = new GradientColorKey(Color.yellow, 0.5f);
        colors[0] = new GradientColorKey(Color.red, 0.0f);

        var alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(0.5f, 0.0f);
        alphas[1] = new GradientAlphaKey(0.5f, 1.0f);

        gradient.SetKeys(colors, alphas);

        SpriteRenderer survivor1 = GameObject.FindGameObjectWithTag("Survivor1").GetComponent<SpriteRenderer>();
        SpriteRenderer survivor2 = GameObject.FindGameObjectWithTag("Survivor2").GetComponent<SpriteRenderer>();
        SpriteRenderer survivor3 = GameObject.FindGameObjectWithTag("Survivor3").GetComponent<SpriteRenderer>();
        SpriteRenderer survivor4 = GameObject.FindGameObjectWithTag("Survivor4").GetComponent<SpriteRenderer>();

        survivor1.color = gradient.Evaluate(survivor1Var.RuntimeValue / 100f);
        survivor2.color = gradient.Evaluate(survivor2Var.RuntimeValue / 100f);
        survivor3.color = gradient.Evaluate(survivor3Var.RuntimeValue / 100f);
        survivor4.color = gradient.Evaluate(survivor4Var.RuntimeValue / 100f);

        Color deathColor = new(0, 0, 0, 0.5f);

        if (survivor1Var.RuntimeValue <= 0) survivor1.color = deathColor;
        if (survivor2Var.RuntimeValue <= 0) survivor2.color = deathColor;
        if (survivor3Var.RuntimeValue <= 0) survivor3.color = deathColor;
        if (survivor4Var.RuntimeValue <= 0) survivor4.color = deathColor;

        GameObject.FindGameObjectWithTag("Day").GetComponent<TMP_Text>().text = "Day " + dayVar.RuntimeValue;
        GameObject.FindGameObjectWithTag("ActionsLeft").GetComponent<TMP_Text>().text = "Actions left: " + actionsLeftVar.RuntimeValue;

        GameObject.FindGameObjectWithTag("WoodCount").GetComponent<TMP_Text>().text = woodVar.RuntimeValue + "x";
        GameObject.FindGameObjectWithTag("Food").GetComponent<Slider>().value = foodVar.RuntimeValue / 100f;
        GameObject.FindGameObjectWithTag("Water").GetComponent<Slider>().value = waterVar.RuntimeValue / 100f;
    }
}