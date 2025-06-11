using System.Collections.Generic;
using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

public class CraftingUIManager : MonoBehaviour
{
    [SerializeField] private GameObject craftingUIPanel;

    private bool isPlayerNear = false;
    public static bool isCraftingUIOpen = false;

    [SerializeField] private Transform recipeListContainer;
    [SerializeField] private GameObject recipeButtonPrefab;

    [SerializeField] private TextMeshProUGUI selectedPlantName;
    [SerializeField] private Image selectedPlantIcon;
    [SerializeField] private Transform materialListContainer;
    [SerializeField] private GameObject materialRequirementPrefab;

    [SerializeField] private Button craftButton;
    [SerializeField] private List<Image> materialIconSlots;

    [SerializeField] private GameObject interactionPromptCanvas;
    [SerializeField] private float promptDistance = 2.5f;
    private Transform player;

    private CraftingSystem craftingSystem;
    private SeedsEnum selectedSeed;

    private void Start()
    {
        craftingSystem = FindObjectOfType<CraftingSystem>();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (interactionPromptCanvas != null)
            interactionPromptCanvas.SetActive(false);

        PopulateRecipeList();
    }

    private void Update()
    {
        if (isPlayerNear)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ToggleCraftingUI();
            }
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (interactionPromptCanvas != null)
            interactionPromptCanvas.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            isPlayerNear = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (isCraftingUIOpen)
                CloseCraftingUI();
        }
    }

    private void ToggleCraftingUI()
    {
        if (isCraftingUIOpen)
        {
            CloseCraftingUI();
        }
        else
        {
            OpenCraftingUI();
        }
    }

    private void OpenCraftingUI()
    {
        craftingUIPanel.SetActive(true);
        isCraftingUIOpen = true;

        GameManager.Instance?.SetGameState(GameState.OnCrafting);
    }

    private void CloseCraftingUI()
    {
        craftingUIPanel.SetActive(false);
        isCraftingUIOpen = false;

        if (GameManager.Instance?.GetCurrentGameState() == GameState.OnCrafting)
        {
            GameManager.Instance.SetGameState(GameState.Digging);
        }
    }

    private void PopulateRecipeList()
    {
        var recipes = craftingSystem.GetAllAvailableRecipes();

        foreach (var recipe in recipes)
        {
            var btn = Instantiate(recipeButtonPrefab, recipeListContainer);
            var plantData = craftingSystem.GetPlantData(recipe.SeedToCraft);

            if (plantData != null)
            {
                var nameText = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                    nameText.text = plantData.plantName;

                var iconTransform = btn.transform.Find("SeedIcon");
                if (iconTransform != null)
                {
                    var iconImage = iconTransform.GetComponent<Image>();
                    if (iconImage != null)
                    {
                        iconImage.sprite = recipe.SeedIcon;
                        iconImage.preserveAspect = true;
                    }
                }

                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SelectRecipe(recipe);
                });
            }
        }
    }

    private void SelectRecipe(CraftingRecipeSeedData recipe)
    {
        selectedSeed = recipe.SeedToCraft;

        var plantData = craftingSystem.GetPlantData(selectedSeed);
        selectedPlantName.text = plantData.plantName;
        selectedPlantIcon.sprite = plantData.plantIcon;
        selectedPlantIcon.preserveAspect = true;

        // Limpiar lista de materiales visuales (texto)
        foreach (Transform child in materialListContainer)
            Destroy(child.gameObject);

        // Mostrar solo textos de requerimientos en la lista
        for (int i = 0; i < recipe.MaterialsRequired.Count; i++)
        {
            var mat = recipe.MaterialsRequired[i];
            var reqUI = Instantiate(materialRequirementPrefab, materialListContainer);

            CraftingMaterialSO materialData = InventoryManager.Instance.GetMaterialData(mat.materialType);
            if (materialData != null)
            {
                var textComponent = reqUI.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = $"{materialData.materialName} x{mat.quantity}";
                }
            }
        }

        // Mostrar íconos separados (fuera de los slots)
        for (int i = 0; i < materialIconSlots.Count; i++)
        {
            if (materialIconSlots[i] == null)
                continue;

            if (i < recipe.MaterialsRequired.Count)
            {
                var mat = recipe.MaterialsRequired[i];
                var materialData = InventoryManager.Instance.GetMaterialData(mat.materialType);

                if (materialData != null && materialData.materialIcon != null)
                {
                    materialIconSlots[i].sprite = materialData.materialIcon;
                    materialIconSlots[i].preserveAspect = true;
                    materialIconSlots[i].SetNativeSize();
                    materialIconSlots[i].enabled = true;
                }
                else
                {
                    materialIconSlots[i].sprite = null;
                    materialIconSlots[i].enabled = false;
                }
            }
            else
            {
                materialIconSlots[i].sprite = null;
                materialIconSlots[i].enabled = false;
            }
        }

        // Habilitar o deshabilitar botón de crafteo
        craftButton.interactable = craftingSystem.HasRequiredMaterials(recipe.MaterialsRequired);
    }

    public void ShowSelectedRecipe(PlantDataSO plantData)
    {
        selectedPlantIcon.sprite = plantData.plantIcon;
        selectedPlantName.text = plantData.plantName;
    }

    public void OnCraftButtonPressed()
    {
        craftingSystem.CraftSeed(selectedSeed);
        SelectRecipe(craftingSystem.GetRecipe(selectedSeed));
    }
}