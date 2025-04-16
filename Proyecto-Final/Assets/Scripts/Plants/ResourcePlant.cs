using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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

    private SpriteRenderer plantRenderer;
    private Color originalColor;
    public Color highlightColor;
    public Color clickColor;

    private int lastProductionDay = 0;
    private bool isProducing = false;
    private bool isReadyToHarvest = false;
    private bool isBeingHarvested = false;

    protected override void Start()
    {
        base.Start();

        plantRenderer = GetComponent<SpriteRenderer>();
        originalColor = plantRenderer.color;

        lastProductionDay = GameManager.Instance.GetCurrentDay();

        if (harvestProgressIndicator != null)
        {
            harvestProgressIndicator.SetActive(false);
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
        return materialSprite;
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

    //void OnMouseDown()
    //{
    //    if (!isReadyToHarvest || isBeingHarvested || abilitySystem == null) return;

    //    GameObject player = GameObject.FindGameObjectWithTag("Player");
    //    if (player != null)
    //    {
    //        float distance = Vector2.Distance(transform.position, player.transform.position);
    //        if (distance <= abilitySystem.interactionDistance)
    //        {
    //            plantRenderer.color = clickColor;

    //            StartHarvest();
    //        }
    //        else
    //        {
    //            plantRenderer.color = originalColor;
    //        }
    //    }
    //}

}