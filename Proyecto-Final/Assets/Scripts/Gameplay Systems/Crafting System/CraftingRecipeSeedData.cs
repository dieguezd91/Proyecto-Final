using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Crafting/Seed Recipe")]
public class CraftingRecipeSeedData : ScriptableObject
{
    [SerializeField] private SeedsEnum seedToCraft;
    [SerializeField] private List<MaterialRequirement> materialsRequired;

    public SeedsEnum SeedToCraft => seedToCraft;
    public List<MaterialRequirement> MaterialsRequired => materialsRequired;
}
