using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUIManager : MonoBehaviour
{
    [SerializeField] private GameObject craftingUIPanel;

    private bool isPlayerNear = false;

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
                OpenCraftingUI();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseCraftingUI();
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
            CloseCraftingUI();
        }
    }

    private void OpenCraftingUI()
    {
        craftingUIPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void CloseCraftingUI()
    {
        craftingUIPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void PopulateRecipeList()
    {
        var recipes = craftingSystem.GetAllAvailableRecipes();

        foreach (var recipe in recipes)
        {
            var btn = Instantiate(recipeButtonPrefab, recipeListContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = recipe.SeedToCraft.ToString();

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                SelectRecipe(recipe);
            });
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
            reqUI.GetComponentInChildren<TextMeshProUGUI>().text = $"{mat.materialType} x{mat.quantity}";
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