using System.Collections.Generic;
using UnityEngine;

public enum SeedsEnum
{
    None = 0,
    // Production Plants
    MoonTear,
    CrimsonFruit,
    DarkRoot,
    EtherBloom,
    SpectralMushroom,
    CyclonicVine,

    // Defensive Plants
    ThornedTendrils,
    FireFlower,
    ShadowIvy,
    ExplosiveBulb,
    IceLotus,
    MoonLily,
    StellarOrchid,

    // Hybrid Plants
    FrostThorn,
    StormRose,
    VolcanicMushroom,
    AstralWaterLily
}

[System.Serializable]
public class MaterialRequirement
{
    public MaterialType materialType;
    public int quantity;
}

public class CraftingSystem : MonoBehaviour
{
    [SerializeField] public CraftingRecipesSeedsListSO craftingRecipes;
    [SerializeField] private List<PlantDataSO> plantDataList;

    private Dictionary<SeedsEnum, PlantDataSO> seedToPlantData = new Dictionary<SeedsEnum, PlantDataSO>();

    private void Awake()
    {
        foreach (var plantData in plantDataList)
        {
            if (plantData.seedType != SeedsEnum.None)
            {
                seedToPlantData[plantData.seedType] = plantData;
            }
        }
    }

    public void CraftSeed(SeedsEnum seedToCraft)
    {
        if (craftingRecipes == null)
        {
            return;
        }

        CraftingRecipeSeedData recipe = craftingRecipes.recipes.Find(r => r.SeedToCraft == seedToCraft);

        if (recipe == null)
        {
            return;
        }

        if (HasRequiredMaterials(recipe.MaterialsRequired))
        {
            ConsumeMaterials(recipe.MaterialsRequired);

            if (seedToPlantData.TryGetValue(seedToCraft, out PlantDataSO plantData))
            {
                int slotIndex = FindFreeSlotOrSpecific(plantData);

                var existingSlot = FindSlotBySeed(seedToCraft);
                if (existingSlot == -1)
                {
                    if (slotIndex != -1)
                    {
                        SeedInventory.Instance.UnlockPlant(
                            plantData.seedType,
                            plantData.plantPrefab,
                            plantData.plantName,
                            plantData.plantIcon,
                            slotIndex,
                            plantData.daysToGrow,
                            plantData.description
                        );
                        SeedInventory.Instance.GetPlantSlot(slotIndex).seedCount = 1;
                    }
                }
                else
                {
                    SeedInventory.Instance.GetPlantSlot(existingSlot).seedCount++;
                    GameManager.Instance.uiManager.UpdateSeedCountsUI();
                }


                if (GameManager.Instance?.uiManager != null)
                {
                    GameManager.Instance.uiManager.InitializeSlotUI();
                }

            }

            if (GameManager.Instance?.uiManager?.inventoryUI != null)
            {
                GameManager.Instance.uiManager.inventoryUI.UpdateAllSlots();
            }
        }
    }

    private int FindFreeSlotOrSpecific(PlantDataSO plantData)
    {
        for (int i = 0; i < 5; i++)
        {
            PlantSlot slot = SeedInventory.Instance.GetPlantSlot(i);
            if (slot.plantPrefab == null && string.IsNullOrEmpty(slot.plantName))
            {
                return i;
            }
        }

        return -1;
    }

    public bool HasRequiredMaterials(List<MaterialRequirement> materialsRequired)
    {
        if (InventoryManager.Instance == null) return false;

        foreach (var requirement in materialsRequired)
        {
            if (!InventoryManager.Instance.HasEnoughMaterial(requirement.materialType, requirement.quantity))
            {
                return false;
            }
        }
        return true;
    }

    private void ConsumeMaterials(List<MaterialRequirement> materialsRequired)
    {
        if (InventoryManager.Instance == null) return;

        foreach (var requirement in materialsRequired)
        {
            InventoryManager.Instance.UseMaterial(requirement.materialType, requirement.quantity);
        }
    }

    public CraftingRecipeSeedData GetRecipe(SeedsEnum seedType)
    {
        if (craftingRecipes == null || craftingRecipes.recipes == null) return null;
        return craftingRecipes.recipes.Find(r => r.SeedToCraft == seedType);
    }

    public bool HasRecipeFor(SeedsEnum seedType)
    {
        return GetRecipe(seedType) != null;
    }

    public List<CraftingRecipeSeedData> GetAllAvailableRecipes()
    {
        if (craftingRecipes == null || craftingRecipes.recipes == null)
            return new List<CraftingRecipeSeedData>();

        return craftingRecipes.recipes;
    }

    public PlantDataSO GetPlantData(SeedsEnum seedType)
    {
        if (seedToPlantData.TryGetValue(seedType, out var data))
            return data;

        return null;
    }

    private int FindSlotBySeed(SeedsEnum seed)
    {
        for (int i = 0; i < 5; i++)
        {
            var slot = SeedInventory.Instance.GetPlantSlot(i);
            if (slot != null && slot.seedType == seed && slot.plantPrefab != null)
                return i;
        }
        return -1;
    }
}