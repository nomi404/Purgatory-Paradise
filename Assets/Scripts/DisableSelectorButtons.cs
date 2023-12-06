using UnityEngine;
using UnityEngine.UI;

public class DisableSelectorButtons : MonoBehaviour
{
    public void Disable()
    {
        var buttons = transform.parent.parent.GetComponentsInChildren<Button>();

        foreach (var b in buttons)
        {
            b.interactable = false;
        }
    }
}