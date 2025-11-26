using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

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
    private RecipeButton _currentSelectedButton = null;

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

        // Ensure the UI shows a recipe when opened: if we previously had a selection, reapply it; otherwise select the first available recipe
        if (craftingSystem != null)
        {
            if (hasSelectedRecipe)
            {
                var existingRecipe = craftingSystem.GetRecipe(selectedSeed);
                if (existingRecipe != null)
                {
                    SelectRecipe(existingRecipe);
                    // Ensure UI layout has updated before toggling visuals
                    Canvas.ForceUpdateCanvases();
                    // Ensure the button visuals are set
                    var btn = FindButtonForRecipe(existingRecipe);
                    if (btn != null)
                    {
                        if (_currentSelectedButton != null && _currentSelectedButton != btn) _currentSelectedButton.SetSelected(false);
                        btn.SetSelected(true);
                        // ensure the EventSystem also marks it selected so built-in visuals apply
                        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(btn.gameObject);
                        // Execute pointer enter so hover visuals/sprites activate
                        ExecuteEvents.Execute(btn.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
                        _currentSelectedButton = btn;
                    }
                }
            }
            else
            {
                var recipes = craftingSystem.GetAllAvailableRecipes();
                foreach (var r in recipes)
                {
                    SelectRecipe(r);
                    // Ensure UI layout has updated before toggling visuals
                    Canvas.ForceUpdateCanvases();
                    var btn = FindButtonForRecipe(r);
                    if (btn != null)
                    {
                        if (_currentSelectedButton != null && _currentSelectedButton != btn) _currentSelectedButton.SetSelected(false);
                        btn.SetSelected(true);
                        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(btn.gameObject);
                        ExecuteEvents.Execute(btn.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
                        _currentSelectedButton = btn;
                    }
                    break;
                }
            }
        }

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

        // Track the first recipe so we can explicitly select it and populate the UI
        CraftingRecipeSeedData firstRecipe = null;
        foreach (var recipe in recipes)
        {
            if (firstRecipe == null) firstRecipe = recipe;
            CreateRecipeButton(recipe);
        }

        // If there is at least one recipe, explicitly select it so the materials and craft button are populated.
        if (firstRecipe != null)
        {
            // Select on the next frame to avoid initialization/timing issues that prevent UI from updating immediately
            StartCoroutine(SelectFirstRecipeNextFrame(firstRecipe));
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

        // register handler passing the button reference so we can manage visual selection
        recipeBtn.OnClick.AddListener(() => OnRecipeButtonClicked(recipeBtn, recipe));

        recipeButtons.Add(recipeBtn);
    }

    // Helper to find the RecipeButton instance for a given recipe
    private RecipeButton FindButtonForRecipe(CraftingRecipeSeedData recipe)
    {
        if (recipe == null) return null;
        foreach (var rb in recipeButtons)
        {
            if (rb == null) continue;
            var data = rb.GetRecipeData();
            if (data != null && data.SeedToCraft == recipe.SeedToCraft)
                return rb;
        }
        return null;
    }

    private void OnRecipeButtonClicked(RecipeButton btn, CraftingRecipeSeedData recipe)
    {
        SelectRecipe(recipe);
        // update visual selection
        if (_currentSelectedButton != null && _currentSelectedButton != btn)
            _currentSelectedButton.SetSelected(false);
        btn.SetSelected(true);
        _currentSelectedButton = btn;
        // ensure the EventSystem and hover visuals reflect selection
        EventSystem.current?.SetSelectedGameObject(btn.gameObject);
        ExecuteEvents.Execute(btn.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
    }

    private void CleanupRecipeButtons()
    {
        foreach (var btn in recipeButtons)
        {
            if (btn != null)
                btn.OnClick.RemoveAllListeners();
            // reset selection visuals
            btn.SetSelected(false);
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

        // Ensure the corresponding RecipeButton is highlighted/selected visually
        HighlightRecipeButton(recipe);
    }

    // Marks the recipe button corresponding to the provided recipe as selected in the UI
    private void HighlightRecipeButton(CraftingRecipeSeedData recipe)
    {
        if (recipe == null) return;

        RecipeButton target = null;
        foreach (var rb in recipeButtons)
        {
            if (rb == null) continue;
            var data = rb.GetRecipeData();
            if (data != null && data.SeedToCraft == recipe.SeedToCraft)
            {
                target = rb;
                break;
            }
        }

        if (target == null) return;

        // Clear prior selected visual
        if (_currentSelectedButton != null && _currentSelectedButton != target)
        {
            _currentSelectedButton.SetSelected(false);
        }

        // Set new selected visual and cache it
        target.SetSelected(true);
        _currentSelectedButton = target;

        // Also trigger the hover event so any hover visuals/audio fire
        target.TriggerHover();
        // ensure pointer enter is executed so UI visuals that depend on it update
        ExecuteEvents.Execute(target.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
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
        // Do not clear hasSelectedRecipe here so we keep track of an existing selection
        // Reset only the visible recipe details (name/icon/description)
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

    private System.Collections.IEnumerator SelectFirstRecipeNextFrame(CraftingRecipeSeedData recipe)
    {
        yield return null; // wait one frame
        if (recipe != null)
        {
            SelectRecipe(recipe);
            var btn = FindButtonForRecipe(recipe);
            if (btn != null)
            {
                if (_currentSelectedButton != null && _currentSelectedButton != btn) _currentSelectedButton.SetSelected(false);
                btn.SetSelected(true);
                // ensure the EventSystem also marks it selected so built-in visuals apply
                UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(btn.gameObject);
                // Execute pointer enter so hover visuals/sprites activate
                ExecuteEvents.Execute(btn.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
                _currentSelectedButton = btn;
            }
            else
            {
                // fallback: ensure HighlightRecipeButton tries to set selection
                HighlightRecipeButton(recipe);
            }
        }
    }

    private void Update()
    {
        if (!isCraftingUIOpen) return;

        if (Input.GetMouseButtonDown(0))
        {
            // Raycast UI elements under the pointer
            var es = EventSystem.current;
            if (es == null) return;

            var ped = new PointerEventData(es) { position = Input.mousePosition };
            var results = new List<RaycastResult>();
            es.RaycastAll(ped, results);

            // If any RecipeButton was clicked, do nothing (its own handler will manage selection)
            bool clickedRecipeButton = results.Any(r => r.gameObject.GetComponentInParent<RecipeButton>() != null);

            // If the click was NOT on a recipe button, restore the current selection visuals
            if (!clickedRecipeButton && _currentSelectedButton != null)
            {
                _currentSelectedButton.SetSelected(true);
                _currentSelectedButton.TriggerHover();
                es.SetSelectedGameObject(_currentSelectedButton.gameObject);
            }
        }
    }
}