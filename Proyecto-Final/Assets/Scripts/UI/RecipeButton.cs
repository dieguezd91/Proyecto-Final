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

    public void SetSelected(bool selected)
    {
        // Rely on the Button component's transition (Sprite Swap) to display selected state.
        var button = GetComponent<Button>();
        if (button != null && selected)
            button.Select();
    }
    #endregion

    #region Public API
    public CraftingRecipeSeedData GetRecipeData() => recipeData;
    public PlantDataSO GetPlantData() => plantData;
    #endregion
}