using CharlesEngine;
using UnityEngine;
[RequireComponent(typeof(CEGameObject))]
[AddComponentMenu("CE Toolbox/Object Visible If")]
public class ObjectVisibleIf : MonoBehaviour
{
    public Condition Condition;

    public void Refresh()
    {
        if (Condition.Eval())
        {
            GetComponent<CEGameObject>().Show();
        }
        else
        {
            GetComponent<CEGameObject>().Hide();
        }
    }
}