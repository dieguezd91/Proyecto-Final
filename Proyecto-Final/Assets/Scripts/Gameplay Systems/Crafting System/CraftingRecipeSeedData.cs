using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Crafting/Seed Recipe")]
public class CraftingRecipeSeedData : ScriptableObject
{
    [SerializeField] private SeedsEnum seedToCraft;
    [SerializeField] private List<MaterialRequirement> materialsRequired;
    [SerializeField] private Sprite seedIcon;

    public SeedsEnum SeedToCraft => seedToCraft;
    public List<MaterialRequirement> MaterialsRequired => materialsRequired;
    public Sprite SeedIcon => seedIcon;
}
