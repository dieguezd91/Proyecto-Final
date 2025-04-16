using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Crafting Recipes", menuName = "Crafting/Crafting Seeds Recipes")]
public class CraftingRecipesSeedsListSO : ScriptableObject
{
    public List<CraftingRecipeSeedData> recipes;
}