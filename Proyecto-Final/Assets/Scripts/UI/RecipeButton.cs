using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeButton : ImprovedUIButton
{
    [Header("Recipe Button Components")]
    [SerializeField] private Image seedIcon;
    [SerializeField] private TextMeshProUGUI seedNameText;

    private CraftingRecipeSeedData recipeData;
    private PlantDataSO plantData;

    #region Setup
    public void Setup(CraftingRecipeSeedData recipe, PlantDataSO plant)
    {
        recipeData = recipe;
        plantData = plant;

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (plantData == null) return;

        if (seedNameText != null)
            seedNameText.text = plantData.plantName;

        if (seedIcon != null && recipeData.SeedIcon != null)
        {
            seedIcon.sprite = recipeData.SeedIcon;
            seedIcon.preserveAspect = true;
        }
    }
    #endregion

    #region Public API
    public CraftingRecipeSeedData GetRecipeData() => recipeData;
    public PlantDataSO GetPlantData() => plantData;
    #endregion
}