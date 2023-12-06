using CharlesEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class Night : MonoBehaviour
{
    public IntVariable actionsLeftVar;
    public BoolVariable lookedForWoodVar;
    public BoolVariable lookedForWaterVar;
    public BoolVariable lookedForFoodVar;

    public BoolVariable shelterVar;
    public BoolVariable weaponsVar;

    public IntVariable survivor1Var;
    public IntVariable survivor2Var;
    public IntVariable survivor3Var;
    public IntVariable survivor4Var;

    public IntVariable dayVar;

    public IntVariable foodVar;
    public IntVariable waterVar;

    private void Start()
    {
        actionsLeftVar.RuntimeValue = 3;
        lookedForWoodVar.RuntimeValue = false;
        lookedForWaterVar.RuntimeValue = false;
        lookedForFoodVar.RuntimeValue = false;

        Dictionary<int, IntVariable> members = new() { { 1, survivor1Var }, { 2, survivor2Var }, { 3, survivor3Var }, { 4, survivor4Var } };
        int membersCount = members.Count(x => x.Value.RuntimeValue > 0);

        int weakestMember = members.Where(x => x.Value.RuntimeValue > 0).OrderBy(x => x.Value.RuntimeValue).First().Key;
        int secondWeakestMember = -1;
        if (membersCount > 1) secondWeakestMember = members.Where(x => x.Value.RuntimeValue > 0 && x.Key != weakestMember).OrderBy(x => x.Value.RuntimeValue).First().Key;

        StringBuilder sb = new();

        if (dayVar.RuntimeValue > 2)
        {
            //wolf attack
            if (Random.Range(0, 10) % 3 == 0)
            {
                sb.AppendLine("Danger lurks in the shadows. Tonight, wolves approached your camp.");
                if (shelterVar.RuntimeValue && weaponsVar.RuntimeValue)
                {
                    sb.AppendLine("Your well-fortified camp with sturdy shelters and weapons repelled the wolves. No harm was done.");
                }
                else if (shelterVar.RuntimeValue)
                {
                    sb.AppendLine("Despite having shelter, the wolves managed to breach your defenses. One member was harmed during the confrontation.");
                    members[weakestMember].RuntimeValue -= 30;
                }
                else if (weaponsVar.RuntimeValue && membersCount > 1)
                {
                    sb.AppendLine("Armed but lacking proper shelter, two members faced the brunt of the wolf attack. They were hurt during the encounter.");
                    members[weakestMember].RuntimeValue -= 30;
                    members[secondWeakestMember].RuntimeValue -= 30;
                }
                else if (weaponsVar.RuntimeValue)
                {
                    sb.AppendLine("Armed but lacking proper shelter, one member faced the brunt of the wolf attack.");
                    members[weakestMember].RuntimeValue -= 50;
                }
                else
                {
                    sb.AppendLine("The night was treacherous, and without adequate shelter or weapons, the wolves overpowered your camp. Tragically, one of your weakest members succumbed to the relentless attack and did not survive the night.");
                    members[weakestMember].RuntimeValue -= 100;
                    membersCount--;
                }
            }
            //bear attack
            else if (Random.Range(0, 10) % 5 == 0)
            {
                sb.AppendLine("The looming threat of a bear materialized tonight. The outcome hinges on your preparedness:");
                if (shelterVar.RuntimeValue && weaponsVar.RuntimeValue)
                {
                    sb.AppendLine("Despite the formidable bear, your well-fortified camp with sturdy shelters and weapons managed to fend it off. However, one member was still hurt during the confrontation.");
                    members[weakestMember].RuntimeValue -= 30;
                }
                else if (shelterVar.RuntimeValue && membersCount > 1)
                {
                    sb.AppendLine("The bear breached your defenses, causing harm to two members despite the presence of shelter.");
                    members[weakestMember].RuntimeValue -= 30;
                    members[secondWeakestMember].RuntimeValue -= 30;
                }
                else if (shelterVar.RuntimeValue)
                {
                    sb.AppendLine("The bear breached your defenses, causing harm to one member despite the presence of shelter.");
                    members[weakestMember].RuntimeValue -= 50;
                }
                else if (weaponsVar.RuntimeValue)
                {
                    sb.AppendLine("Armed but lacking proper shelter, your group faced the powerful bear. Tragically, one member could not withstand the ferocity of the attack and did not survive.");
                    members[weakestMember].RuntimeValue -= 100;
                    membersCount--;
                }
                else if (membersCount > 1)
                {
                    sb.AppendLine("The bear's onslaught was too much to bear without adequate defenses. Two members tragically lost their lives during the ruthless attack.");
                    members[weakestMember].RuntimeValue -= 100;
                    members[secondWeakestMember].RuntimeValue -= 100;
                    membersCount -= 2;
                }
                else
                {
                    sb.AppendLine("The bear's onslaught was too much to bear without adequate defenses. One member tragically lost his live during the ruthless attack.");
                    members[weakestMember].RuntimeValue -= 100;
                    membersCount--;
                }
            }
        }

        int membersAfterAttack = membersCount;

        foreach (var member in members)
        {
            if (member.Value.RuntimeValue <= 0) continue;

            if (waterVar.RuntimeValue > 0)
            {
                member.Value.RuntimeValue += Mathf.Min(waterVar.RuntimeValue, 5);
                waterVar.RuntimeValue -= Mathf.Min(waterVar.RuntimeValue, 5);
            }

            if (foodVar.RuntimeValue > 0)
            {
                member.Value.RuntimeValue += Mathf.Min(foodVar.RuntimeValue, 5);
                foodVar.RuntimeValue -= Mathf.Min(foodVar.RuntimeValue, 5);
            }

            member.Value.RuntimeValue -= 15;

            if (member.Value.RuntimeValue <= 0) membersCount--;
        }

        if (membersCount <= 0)
        {
            sb.AppendLine();
            sb.AppendLine("All members have tragically perished, leaving the camp silent and empty.");
        }
        else if (membersAfterAttack == membersCount + 1)
        {
            sb.AppendLine();
            sb.AppendLine("Despite your best efforts, a member of your group succumbed to exhaustion, dehydration, or starvation.");
        }
        else if (membersAfterAttack > membersCount)
        {
            sb.AppendLine();
            sb.AppendLine("Despite your best efforts, some members of your group succumbed to exhaustion, dehydration, or starvation.");
        }

        dayVar.IncrementValue();

        if (string.IsNullOrEmpty(sb.ToString())) sb.Append("The night passed without incident, but the silent darkness took a toll on the survivors. Lacking restorative rest and sustenance, your group grows weaker. The shadows may be dormant, but the struggle for survival endures.");

        GetComponent<TMP_Text>().text = sb.ToString();
    }
}