using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftingRecipes", menuName = "Crafting/CraftingSeedsRecipes")]
public class CraftingRecipesSeedsListSO : ScriptableObject
{
    public List<CraftingRecipeSeedData> recipes;
}
