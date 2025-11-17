using UnityEngine;
using System.Collections;

[System.Serializable]
public class HarvestReward
{
    public string materialName;
    public int amount;
    public Sprite icon;
}

public class ResourcePlant : Plant
{
    [Header("RESOURCES SETTINGS")]
    [SerializeField] private int daysToProduceResources = 2;
    [SerializeField] private int minimumResourceAmount = 1;
    [SerializeField] private int maximumResourceAmount = 5;
    [SerializeField] private MaterialType materialType;
    [SerializeField] private Sprite materialSprite;

    [Header("HARVEST SETTINGS")]
    [SerializeField] private float harvestDuration = 2f;
    [SerializeField] private GameObject harvestProgressIndicator;

    [Header("HARVEST FX")]
    [SerializeField] private ParticleSystem harvestReadyParticles;

    private SpriteRenderer plantRenderer;
    private Color originalColor;
    public Color highlightColor;
    public Color clickColor;

    private bool isProducing = false;
    private bool isReadyToHarvest = false;
    private bool isBeingHarvested = false;
    private int cycleStartDay = -1;

    [SerializeField] private MaterialType rewardType;
    [SerializeField] private int rewardAmount = 1;

    private PlantGrowthUI growthUI;

    protected override void Start()
    {
        base.Start();

        plantRenderer = GetComponent<SpriteRenderer>();
        originalColor = plantRenderer.color;

        cycleStartDay = LevelManager.Instance.GetCurrentDay();
        growthUI = GetComponentInChildren<PlantGrowthUI>();

        if (harvestProgressIndicator != null)
        {
            harvestProgressIndicator.SetActive(false);
        }
    }

    protected override void OnMature()
    {
        base.OnMature();
        cycleStartDay = LevelManager.Instance.GetCurrentDay();
    }

    private void CheckProduction(int currentDay)
    {
        if (!IsFullyGrown() || isProducing || isReadyToHarvest || isBeingHarvested)
            return;

        int daysSinceCycleStart = currentDay - cycleStartDay;
        if (daysSinceCycleStart >= daysToProduceResources)
        {
            ProduceResources();
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

        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive())
        {
            TutorialEvents.InvokeFirstPlantReadyToHarvest();
        }

        ActivateHarvestReadyParticles();
    }

    private void ActivateHarvestReadyParticles()
    {
        if (harvestReadyParticles != null && !harvestReadyParticles.isPlaying)
            harvestReadyParticles.Play();
    }

    public void StartHarvest()
    {
        if (!isReadyToHarvest || isBeingHarvested)
            return;

        if (LevelManager.Instance.currentGameState != GameState.Harvesting)
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
        }

        float harvestTimer = 0f;
        while (harvestTimer < harvestDuration)
        {
            if (LevelManager.Instance.currentGameState != GameState.Harvesting)
            {
                CancelHarvest();
                yield break;
            }

            harvestTimer += Time.deltaTime;

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

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddMaterial(materialType, resourceAmount);

            InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
            if (inventoryUI != null && inventoryUI.gameObject.activeInHierarchy)
            {
                inventoryUI.UpdateAllSlots();
            }
        }

        Sprite resourceSprite = GetResourceSprite();
        if (resourceSprite != null)
        {
            InventoryManager.Instance.SetMaterialIcon(materialType, resourceSprite);
        }

        if (harvestProgressIndicator != null)
        {
            harvestProgressIndicator.SetActive(false);
        }

        isReadyToHarvest = false;
        isBeingHarvested = false;
        cycleStartDay = LevelManager.Instance.GetCurrentDay();

        DeactivateHarvestReadyParticles();

        if (growthUI != null)
        {
            growthUI.UpdateProgressUI();
        }
    }

    private void DeactivateHarvestReadyParticles()
    {
        if (harvestReadyParticles != null && harvestReadyParticles.isPlaying)
            harvestReadyParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    protected override void HandleGameStateChanged(GameState newState)
    {
         if (newState == GameState.Night)
         {
             base.HandleGameStateChanged(newState);
             CheckProduction(LevelManager.Instance.GetCurrentDay());
         }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (LevelManager.Instance != null)
            LevelManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
    }

    void OnMouseOver()
    {
        if (isReadyToHarvest)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                if (abilitySystem != null && distance <= abilitySystem.interactionDistance)
                {
                    plantRenderer.color = highlightColor;
                }
                else
                {
                    plantRenderer.color = originalColor;
                }
            }
        }
    }

    void OnMouseExit()
    {
        if (!isBeingHarvested)
        {
            plantRenderer.color = originalColor;
        }
    }

    public float GetTotalProgress()
    {
        if (cycleStartDay < 0)
            return 0f;

        if (isReadyToHarvest)
            return 1f;

        int currentDay = LevelManager.Instance.GetCurrentDay();
        int elapsed = currentDay - cycleStartDay;

        int required = IsFullyGrown() ? daysToProduceResources : plantData.daysToGrow;

        if (required <= 0)
        {
            return (elapsed <= 0) ? 0f : 1f;
        }

        return Mathf.Clamp01((float)elapsed / required);
    }

    public HarvestReward GetHarvestReward()
    {
        if (InventoryManager.Instance != null)
        {
            string name = InventoryManager.Instance.GetMaterialName(rewardType);
            Sprite icon = InventoryManager.Instance.GetMaterialIcon(rewardType);
            return new HarvestReward
            {
                materialName = name,
                amount = rewardAmount,
                icon = icon
            };
        }
        return null;
    }


    public int GetLastProductionDay() => cycleStartDay;

    public int GetDaysToProduce() => daysToProduceResources;

    private Sprite GetResourceSprite() => materialSprite;

    public bool IsReadyToHarvest() => isReadyToHarvest;

    public bool IsBeingHarvested() => isBeingHarvested;

    public float GetHarvestDuration() => harvestDuration;

    public void CompletedHarvest() => CompleteHarvest();
}