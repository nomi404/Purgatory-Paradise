using UnityEngine;

public class DisableParent : MonoBehaviour
{
    public void Disable()
    {
        transform.parent.gameObject.SetActive(false);
    }
}