using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject craftingUIPanel;

    [Header("Recipe List")]
    [SerializeField] private Transform recipeListContainer;
    [SerializeField] private GameObject recipeButtonPrefab;

    [Header("Selected Recipe Display")]
    [SerializeField] private TextMeshProUGUI selectedPlantName;
    [SerializeField] private Image selectedPlantIcon;
    [SerializeField] private TextMeshProUGUI selectedPlantDescription;

    [Header("Materials Display")]
    [SerializeField] private Transform materialListContainer;
    [SerializeField] private GameObject materialRequirementPrefab;
    [SerializeField] private List<Image> materialIconSlots;

    [Header("Action Buttons")]
    [SerializeField] private CraftButton craftButton;
    [SerializeField] private CloseButton closeButton;

    private CraftingSystem craftingSystem;
    private SeedsEnum selectedSeed;
    private List<RecipeButton> recipeButtons = new List<RecipeButton>();
    private bool hasSelectedRecipe = false;

    public static bool isCraftingUIOpen = false;

    #region Unity Lifecycle
    private void Start()
    {
        InitializeReferences();
        SubscribeToEvents();
        PopulateRecipeList();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    #endregion

    #region Initialization
    private void InitializeReferences()
    {
        craftingSystem = FindObjectOfType<CraftingSystem>();
    }

    private void SubscribeToEvents()
    {
        UIEvents.OnCraftingUIToggleRequested += ToggleCraftingUI;

        if (craftButton != null)
            craftButton.OnClick.AddListener(HandleCraftButtonClick);

        if (closeButton != null)
            closeButton.OnClick.AddListener(CloseCraftingUI);
    }

    private void UnsubscribeFromEvents()
    {
        UIEvents.OnCraftingUIToggleRequested -= ToggleCraftingUI;

        if (craftButton != null)
            craftButton.OnClick.RemoveListener(HandleCraftButtonClick);

        if (closeButton != null)
            closeButton.OnClick.RemoveListener(CloseCraftingUI);

        CleanupRecipeButtons();
    }
    #endregion

    #region UI Toggle
    private void ToggleCraftingUI()
    {
        if (isCraftingUIOpen)
            CloseCraftingUI();
        else
            OpenCraftingUI();
    }

    private void OpenCraftingUI()
    {
        craftingUIPanel.SetActive(true);
        isCraftingUIOpen = true;

        ResetRecipeDisplay();
        HideCraftButton();

        LevelManager.Instance?.SetGameState(GameState.OnCrafting);
    }

    public void CloseCraftingUI()
    {
        craftingUIPanel.SetActive(false);
        isCraftingUIOpen = false;

        ClearMaterialList();

        if (LevelManager.Instance?.GetCurrentGameState() == GameState.OnCrafting)
            LevelManager.Instance.SetGameState(GameState.Digging);

        UIEvents.TriggerCraftingUIClosed();
        TutorialEvents.InvokeCraftingClosed();
    }
    #endregion

    #region Recipe List Management
    private void PopulateRecipeList()
    {
        CleanupRecipeButtons();

        var recipes = craftingSystem.GetAllAvailableRecipes();
        foreach (var recipe in recipes)
        {
            CreateRecipeButton(recipe);
        }
    }

    private void CreateRecipeButton(CraftingRecipeSeedData recipe)
    {
        GameObject buttonObj = Instantiate(recipeButtonPrefab, recipeListContainer);
        RecipeButton recipeBtn = buttonObj.GetComponent<RecipeButton>();

        if (recipeBtn == null)
        {
            Destroy(buttonObj);
            return;
        }

        var plantData = craftingSystem.GetPlantData(recipe.SeedToCraft);
        recipeBtn.Setup(recipe, plantData);

        recipeBtn.OnClick.AddListener(() => SelectRecipe(recipe));

        recipeButtons.Add(recipeBtn);
    }

    private void CleanupRecipeButtons()
    {
        foreach (var btn in recipeButtons)
        {
            if (btn != null)
                btn.OnClick.RemoveAllListeners();
        }

        recipeButtons.Clear();

        foreach (Transform child in recipeListContainer)
            Destroy(child.gameObject);
    }
    #endregion

    #region Recipe Selection
    private void SelectRecipe(CraftingRecipeSeedData recipe)
    {
        selectedSeed = recipe.SeedToCraft;
        hasSelectedRecipe = true;

        var plantData = craftingSystem.GetPlantData(selectedSeed);

        UpdateRecipeDisplay(plantData, recipe);
        UpdateMaterialsList(recipe);
        UpdateCraftButton(recipe);
    }

    private void UpdateRecipeDisplay(PlantDataSO plantData, CraftingRecipeSeedData recipe)
    {
        selectedPlantName.text = plantData.plantName;

        if (plantData.plantIcon != null)
        {
            selectedPlantIcon.sprite = plantData.plantIcon;
            selectedPlantIcon.preserveAspect = true;
            selectedPlantIcon.enabled = true;
        }
        else
        {
            selectedPlantIcon.enabled = false;
        }

        if (selectedPlantDescription != null)
            selectedPlantDescription.text = plantData.description;
    }

    private void UpdateMaterialsList(CraftingRecipeSeedData recipe)
    {
        ClearMaterialList();

        foreach (var material in recipe.MaterialsRequired)
        {
            CreateMaterialRequirementUI(material);
        }

        UpdateMaterialIconSlots(recipe);
    }

    private void CreateMaterialRequirementUI(MaterialRequirement material)
    {
        GameObject reqUI = Instantiate(materialRequirementPrefab, materialListContainer);
        CraftingMaterialSO materialData = InventoryManager.Instance.GetMaterialData(material.materialType);

        if (materialData == null) return;

        SetupMaterialText(reqUI, materialData, material);
        SetupMaterialIcon(reqUI, materialData);
    }

    private void SetupMaterialText(GameObject reqUI, CraftingMaterialSO materialData, MaterialRequirement material)
    {
        var textComponent = reqUI.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent == null) return;

        int playerAmount = InventoryManager.Instance.GetMaterialAmount(material.materialType);
        textComponent.color = playerAmount >= material.quantity ? Color.green : Color.red;
        textComponent.text = $"{materialData.materialName} x{material.quantity}";
    }

    private void SetupMaterialIcon(GameObject reqUI, CraftingMaterialSO materialData)
    {
        var iconTransform = reqUI.transform.Find("MaterialIcon");
        if (iconTransform == null) return;

        var iconImage = iconTransform.GetComponent<Image>();
        if (iconImage != null && materialData.materialIcon != null)
        {
            iconImage.sprite = materialData.materialIcon;
            iconImage.preserveAspect = true;
            iconImage.enabled = true;
        }
    }

    private void UpdateMaterialIconSlots(CraftingRecipeSeedData recipe)
    {
        for (int i = 0; i < materialIconSlots.Count; i++)
        {
            if (materialIconSlots[i] == null) continue;

            if (i < recipe.MaterialsRequired.Count)
            {
                SetMaterialSlot(materialIconSlots[i], recipe.MaterialsRequired[i]);
            }
            else
            {
                ClearMaterialSlot(materialIconSlots[i]);
            }
        }
    }

    private void SetMaterialSlot(Image slot, MaterialRequirement material)
    {
        var materialData = InventoryManager.Instance.GetMaterialData(material.materialType);

        if (materialData?.materialIcon != null)
        {
            slot.sprite = materialData.materialIcon;
            slot.preserveAspect = true;
            slot.SetNativeSize();
            slot.enabled = true;
        }
        else
        {
            ClearMaterialSlot(slot);
        }
    }

    private void ClearMaterialSlot(Image slot)
    {
        slot.sprite = null;
        slot.enabled = false;
    }

    private void UpdateCraftButton(CraftingRecipeSeedData recipe)
    {
        if (craftButton == null) return;

        craftButton.Setup(recipe);
        ShowCraftButton();
    }
    #endregion

    #region Crafting Action
    private void HandleCraftButtonClick()
    {
        if (!hasSelectedRecipe || craftButton == null) return;

        if (!craftButton.HasRecipe()) return;

        craftingSystem.CraftSeed(selectedSeed);

        var recipe = craftingSystem.GetRecipe(selectedSeed);
        SelectRecipe(recipe);
    }
    #endregion

    private void ShowCraftButton()
    {
        if (craftButton != null)
            craftButton.gameObject.SetActive(true);
    }

    private void HideCraftButton()
    {
        if (craftButton != null)
        {
            craftButton.Clear();
            craftButton.gameObject.SetActive(false);
        }
    }

    #region Helper Methods
    private void ResetRecipeDisplay()
    {
        hasSelectedRecipe = false;

        selectedPlantIcon.enabled = false;
        selectedPlantIcon.sprite = null;
        selectedPlantName.text = "";

        if (selectedPlantDescription != null)
            selectedPlantDescription.text = "";
    }

    private void ClearMaterialList()
    {
        foreach (Transform child in materialListContainer)
            Destroy(child.gameObject);
    }

    public void ShowSelectedRecipe(PlantDataSO plantData)
    {
        selectedPlantIcon.sprite = plantData.plantIcon;
        selectedPlantName.text = plantData.plantName;
    }
    #endregion
}