using UnityEngine;
using CharlesEngine;

public class SurvivorTalkImageSetter : MonoBehaviour
{
    public IntVariable selectedSurvivor;
    public Sprite[] images;

    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = images[selectedSurvivor.RuntimeValue - 1];
    }
}