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
    [SerializeField] private TextMeshProUGUI selectedPlantDescription;
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
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F) && GameManager.Instance.currentGameState != GameState.Night)
        {
            ToggleCraftingUI();
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        //if (interactionPromptCanvas != null)
        //    interactionPromptCanvas.SetActive(false);
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

        selectedPlantIcon.enabled = false;
        selectedPlantIcon.sprite = null;
        selectedPlantName.text = "";
        if (selectedPlantDescription != null)
            selectedPlantDescription.text = "";

        GameManager.Instance?.SetGameState(GameState.OnCrafting);
    }

    private void CloseCraftingUI()
    {
        craftingUIPanel.SetActive(false);
        isCraftingUIOpen = false;

        foreach (Transform child in materialListContainer)
            Destroy(child.gameObject);


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

        if (plantData.plantIcon != null)
        {
            selectedPlantIcon.sprite = plantData.plantIcon;
            selectedPlantIcon.preserveAspect = true;
            selectedPlantIcon.enabled = true;
        }
        else
        {
            selectedPlantIcon.sprite = null;
            selectedPlantIcon.enabled = false;
        }

        if (selectedPlantDescription != null)
            selectedPlantDescription.text = plantData.description;

        foreach (Transform child in materialListContainer)
            Destroy(child.gameObject);

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
                    int playerAmount = InventoryManager.Instance.GetMaterialAmount(mat.materialType);

                    if (playerAmount < mat.quantity)
                    {
                        textComponent.color = Color.red;
                    }
                    else
                    {
                        textComponent.color = Color.green;
                    }

                    textComponent.text = $"{materialData.materialName} x{mat.quantity}";
                }

                var iconTransform = reqUI.transform.Find("MaterialIcon");
                if (iconTransform != null)
                {
                    var iconImage = iconTransform.GetComponent<Image>();
                    if (iconImage != null && materialData.materialIcon != null)
                    {
                        iconImage.sprite = materialData.materialIcon;
                        iconImage.preserveAspect = true;
                        iconImage.enabled = true;
                    }
                }
            }
        }

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