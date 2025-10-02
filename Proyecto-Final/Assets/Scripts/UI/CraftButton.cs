using UnityEngine;

public class CraftButton : ImprovedUIButton
{
    private CraftingRecipeSeedData currentRecipe;
    private CraftingSystem craftingSystem;

    #region Setup
    public void Setup(CraftingRecipeSeedData recipe)
    {
        if (craftingSystem == null)
            craftingSystem = FindObjectOfType<CraftingSystem>();

        currentRecipe = recipe;
        UpdateButtonState();
    }

    public void Clear()
    {
        currentRecipe = null;
        Interactable = false;
    }

    public void UpdateButtonState()
    {
        if (currentRecipe == null)
        {
            Interactable = false;
            return;
        }

        if (craftingSystem == null)
            craftingSystem = FindObjectOfType<CraftingSystem>();

        if (craftingSystem == null)
        {
            Interactable = false;
            return;
        }

        bool hasRequiredMaterials = craftingSystem.HasRequiredMaterials(currentRecipe.MaterialsRequired);
        Interactable = hasRequiredMaterials;
    }
    #endregion

    #region Public API
    public CraftingRecipeSeedData GetCurrentRecipe() => currentRecipe;

    public bool HasRecipe() => currentRecipe != null;
    #endregion
}