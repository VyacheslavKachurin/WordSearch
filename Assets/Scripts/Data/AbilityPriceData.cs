using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityPriceData", menuName = "ScriptableObjects/AbilityPriceData", order = 1)]
public class AbilityPriceData : ScriptableObject
{
    public AbilityPriceItem[] Items;

    public int GetPrice(Ability ability)
    {
        return Items.Where(x => x.Ability == ability).First().Price;
    }
}