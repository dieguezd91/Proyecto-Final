using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ResourcePlant : Plant
{
    [Header("RESOURCES SETTINGS")]
    [SerializeField] private int daysToProduceResources = 2;
    [SerializeField] private int minimumResourceAmount = 1;
    [SerializeField] private int maximumResourceAmount = 5;
    [SerializeField] private string resourceType = "Wood";
    [SerializeField] private Sprite resourceSprite;


    [Header("HARVEST SETTINGS")]
    [SerializeField] private float harvestDuration = 2f;
    [SerializeField] private GameObject harvestProgressIndicator;
    [SerializeField] private Image progressFillImage;

    private int lastProductionDay = 0;
    private bool isProducing = false;
    private bool isReadyToHarvest = false;
    private bool isBeingHarvested = false;

    protected override void Start()
    {
        base.Start();
        lastProductionDay = GameManager.Instance.GetCurrentDay();

        if (harvestProgressIndicator != null)
        {
            harvestProgressIndicator.SetActive(false);

            if (progressFillImage == null)
            {
                progressFillImage = harvestProgressIndicator.GetComponentInChildren<Image>();
            }
        }
    }

    protected override void OnMature()
    {
        base.OnMature();
        GameManager.Instance.onNewDay.AddListener(CheckProduction);
    }

    private void CheckProduction(int currentDay)
    {
        if (IsFullyGrown() && !isProducing && !isReadyToHarvest && !isBeingHarvested)
        {
            int daysSinceLastProduction = currentDay - lastProductionDay;
            if (daysSinceLastProduction >= daysToProduceResources)
            {
                ProduceResources();
            }
        }
    }

    private void ProduceResources()
    {
        isProducing = true;
        StartCoroutine(FinishProduction());
    }

    private IEnumerator FinishProduction()
    {
        yield return new WaitForSeconds(0.5f);

        isProducing = false;
        isReadyToHarvest = true;

        Debug.Log($"Resources ready to harvest");
    }

    public bool IsReadyToHarvest()
    {
        return isReadyToHarvest;
    }

    public bool IsBeingHarvested()
    {
        return isBeingHarvested;
    }

    public float GetHarvestDuration()
    {
        return harvestDuration;
    }

    public void CompletedHarvest()
    {
        CompleteHarvest();
    }

    public void StartHarvest()
    {
        if (!isReadyToHarvest || isBeingHarvested)
            return;

        if (GameManager.Instance.currentGameState != GameState.Day)
        {
            return;
        }

        isBeingHarvested = true;
        StartCoroutine(HarvestCoroutine());
    }

    private IEnumerator HarvestCoroutine()
    {
        if (harvestProgressIndicator != null)
        {
            harvestProgressIndicator.SetActive(true);

            if (progressFillImage != null)
            {
                progressFillImage.fillAmount = 0f;
            }
        }

        float harvestTimer = 0f;
        while (harvestTimer < harvestDuration)
        {
            if (GameManager.Instance.currentGameState != GameState.Day)
            {
                CancelHarvest();
                yield break;
            }

            harvestTimer += Time.deltaTime;

            if (progressFillImage != null)
            {
                float progressPercentage = Mathf.Clamp01(harvestTimer / harvestDuration);

                progressFillImage.fillAmount = progressPercentage;
            }

            yield return null;
        }

        CompleteHarvest();
    }

    public void CancelHarvest()
    {
        isBeingHarvested = false;

        if (harvestProgressIndicator != null)
        {
            harvestProgressIndicator.SetActive(false);
        }
    }

    private void CompleteHarvest()
    {
        if (!isBeingHarvested) return;

        int resourceAmount = Random.Range(minimumResourceAmount, maximumResourceAmount + 1);

        if (ResourceInventory.Instance != null)
        {
            ResourceInventory.Instance.AddResource(resourceType, resourceAmount);

            ResourceInventoryUI inventoryUI = FindObjectOfType<ResourceInventoryUI>();
            if (inventoryUI != null && inventoryUI.gameObject.activeInHierarchy)
            {
                inventoryUI.UpdateAllSlots();
            }
        }

        Sprite resourceSprite = GetResourceSprite();
        if (resourceSprite != null)
        {
            ResourceInventory.Instance.SetResourceIcon(resourceType, resourceSprite);
        }

        Debug.Log($"ResourcePlant: Harvested {resourceAmount} of {resourceType}");

        if (harvestProgressIndicator != null)
        {
            harvestProgressIndicator.SetActive(false);
        }

        isReadyToHarvest = false;
        isBeingHarvested = false;
        lastProductionDay = GameManager.Instance.GetCurrentDay();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (GameManager.Instance != null && IsFullyGrown())
        {
            GameManager.Instance.onNewDay.RemoveListener(CheckProduction);
        }
    }

    private Sprite GetResourceSprite()
    {
        return resourceSprite;
    }

    void OnMouseDown()
    {
        if (isReadyToHarvest && !isBeingHarvested)
        {
            StartHarvest();
        }
    }
}