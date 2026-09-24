using System.Collections;
using UnityEngine;

[System.Serializable]
public class HarvestReward
{
    public string materialName;
    public int amount;
    public Sprite icon;
}

public class HarvestablePlant : MonoBehaviour
{
    [Header("HARVEST SETTINGS")]
    [SerializeField] private float harvestDuration = 2f;
    [SerializeField] private bool startsReadyToHarvest = true;

    [Header("REWARD")]
    [SerializeField] private MaterialType rewardType;
    [SerializeField] private int rewardAmount = 1;
    [SerializeField] private Sprite rewardSprite;

    [Header("VISUAL")]
    public Color highlightColor = Color.white;
    public Color clickColor = Color.white;

    private bool isReadyToHarvest;
    private bool isBeingHarvested;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        isReadyToHarvest = startsReadyToHarvest;
    }

    public void SetReadyToHarvest(bool ready)
    {
        isReadyToHarvest = ready;

        if (!ready)
            CancelHarvest();
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

    public void StartHarvest()
    {
        if (!isReadyToHarvest || isBeingHarvested)
            return;

        PlayerAbilitySystem abilitySystem =
            FindObjectOfType<PlayerAbilitySystem>();

        if (abilitySystem == null ||
            abilitySystem.CurrentAbility != PlayerAbility.Harvesting)
        {
            return;
        }

        isBeingHarvested = true;

        StartCoroutine(HarvestCoroutine());
    }

    private IEnumerator HarvestCoroutine()
    {
        float timer = 0f;

        while (timer < harvestDuration)
        {
            PlayerAbilitySystem abilitySystem =
                FindObjectOfType<PlayerAbilitySystem>();

            if (abilitySystem == null ||
                abilitySystem.CurrentAbility != PlayerAbility.Harvesting)
            {
                CancelHarvest();
                yield break;
            }

            timer += Time.deltaTime;

            yield return null;
        }

        CompletedHarvest();
    }

    public void CancelHarvest()
    {
        isBeingHarvested = false;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    public void CompletedHarvest()
    {
        if (!isBeingHarvested)
            return;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddMaterial(
                rewardType,
                rewardAmount
            );

            InventoryUI inventoryUI =
                FindObjectOfType<InventoryUI>();

            if (inventoryUI != null &&
                inventoryUI.gameObject.activeInHierarchy)
            {
                inventoryUI.UpdateAllSlots();
            }

            Sprite resourceSprite = GetResourceSprite();

            if (resourceSprite != null)
            {
                InventoryManager.Instance.SetMaterialIcon(
                    rewardType,
                    resourceSprite
                );
            }
        }

        isReadyToHarvest = false;
        isBeingHarvested = false;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    public HarvestReward GetHarvestReward()
    {
        if (InventoryManager.Instance != null)
        {
            string name =
                InventoryManager.Instance.GetMaterialName(rewardType);

            Sprite icon =
                InventoryManager.Instance.GetMaterialIcon(rewardType);

            if (icon == null)
                icon = rewardSprite;

            return new HarvestReward
            {
                materialName = name,
                amount = rewardAmount,
                icon = icon
            };
        }

        return new HarvestReward
        {
            materialName = rewardType.ToString(),
            amount = rewardAmount,
            icon = rewardSprite
        };
    }

    public float GetTotalProgress()
    {
        return isReadyToHarvest ? 1f : 0f;
    }

    private Sprite GetResourceSprite()
    {
        if (rewardSprite != null)
            return rewardSprite;

        if (InventoryManager.Instance != null)
            return InventoryManager.Instance.GetMaterialIcon(rewardType);

        return null;
    }

    private void OnMouseOver()
    {
        if (!isReadyToHarvest || isBeingHarvested)
            return;

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        PlayerAbilitySystem abilitySystem =
            player.GetComponent<PlayerAbilitySystem>();

        if (abilitySystem == null)
            return;

        float distance =
            Vector2.Distance(transform.position, player.transform.position);

        if (distance <= abilitySystem.interactionDistance)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = highlightColor;
        }
        else
        {
            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;
        }
    }

    private void OnMouseExit()
    {
        if (!isBeingHarvested && spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
}