using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCraftingRecipe", menuName = "Crafting/SeedRecipe")]
public class CraftingRecipeSeedData : ScriptableObject
{
    [SerializeField] private SeedsEnum seedToCraft;
    [SerializeField] private List<MaterialRequirement> materialsRequired;

    public SeedsEnum SeedToCraft => seedToCraft;
    public List<MaterialRequirement> MaterialsRequired => materialsRequired;
}
