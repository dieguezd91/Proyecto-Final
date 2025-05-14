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

    private CraftingSystem craftingSystem;
    private SeedsEnum selectedSeed;

    private void Start()
    {
        craftingSystem = FindObjectOfType<CraftingSystem>();
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
            GameManager.Instance.SetGameState(GameState.Day);
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

                var iconTransform = btn.transform.Find("PlantImage");
                if (iconTransform != null)
                {
                    var iconImage = iconTransform.GetComponent<Image>();
                    if (iconImage != null)
                    {
                        iconImage.sprite = plantData.plantIcon;
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

        foreach (Transform child in materialListContainer)
            Destroy(child.gameObject);

        foreach (var mat in recipe.MaterialsRequired)
        {
            var reqUI = Instantiate(materialRequirementPrefab, materialListContainer);

            var materialName= InventoryManager.Instance.GetMaterialName(mat.materialType);

            reqUI.GetComponentInChildren<TextMeshProUGUI>().text = $"{materialName} x{mat.quantity}";
        }

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