using UnityEngine;

public class TilePlantInfo
{
    public bool isOccupied;
    public Plant currentPlant;
}

public class Plant : MonoBehaviour
{
    [Header("Plant Data")]
    [SerializeField] protected PlantDataSO plantData;

    [Header("State")]
    [SerializeField] public int plantingDay;
    [SerializeField] private bool growthCompleted = false;

    protected int daysToGrow;
    protected Sprite plantSprite;

    private LifeController lifeController;
    [HideInInspector] public Vector3Int tilePosition;

    private SpriteRenderer spriteRenderer;

    protected Sprite startingDaySprite;
    protected Sprite middleDaySprite;
    protected Sprite lastDaySprite;
    protected Sprite fullyGrownSprite;

    protected PlayerAbilitySystem abilitySystem;

    private Collider2D plantCollider;

    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        plantCollider = GetComponent<Collider2D>();
        if (plantCollider != null)
        {
            plantCollider.enabled = false;
        }

        plantingDay = GameManager.Instance.GetCurrentDay();
        GameManager.Instance.onNewDay.AddListener(OnNewDay);

        abilitySystem = FindObjectOfType<PlayerAbilitySystem>();

        if (plantData != null)
        {
            daysToGrow = plantData.daysToGrow;
            plantSprite = plantData.plantIcon;

            startingDaySprite = plantData.startingDaySprite;
            middleDaySprite = plantData.middleDaySprite;
            lastDaySprite = plantData.lastDaySprite;
            fullyGrownSprite = plantData.fullyGrownSprite;

            ChangeSprite(startingDaySprite);
        }
        else
        {
            Debug.LogWarning($"[Plant] No PlantDataSO assigned on {gameObject.name}");
        }

        lifeController = GetComponent<LifeController>();
        if (lifeController == null)
        {
            lifeController = gameObject.AddComponent<LifeController>();
        }

        lifeController.onDeath.AddListener(HandlePlantDeath);
    }

    protected virtual void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onNewDay.RemoveListener(OnNewDay);
        }

        if (lifeController != null)
        {
            lifeController.onDeath.RemoveListener(HandlePlantDeath);
        }
    }

    protected virtual void OnNewDay(int currentDay)
    {
        UpdateGrowthStatus(currentDay);
    }

    private void UpdateGrowthStatus(int currentDay)
    {
        if (growthCompleted)
            return;

        int daysSincePlanting = currentDay - plantingDay;

        if (daysSincePlanting <= 0)
        {
            ChangeSprite(startingDaySprite);
            return;
        }

        Debug.Log($"Planta: {gameObject.name}, Días desde plantación: {daysSincePlanting}, Días para crecer: {daysToGrow}");

        if (daysSincePlanting >= daysToGrow)
        {
            CompleteGrowth();
            return;
        }

        float progress = Mathf.Clamp01((float)daysSincePlanting / daysToGrow);
        Debug.Log($"Progreso de crecimiento: {progress:F2}");

        if (progress <= 0.33f)
            ChangeSprite(startingDaySprite);
        else if (progress <= 0.66f)
            ChangeSprite(middleDaySprite);
        else if (progress < 1.0f)
            ChangeSprite(lastDaySprite);
    }

    private void CompleteGrowth()
    {
        if (growthCompleted)
            return;

        growthCompleted = true;
        Debug.Log("Plant: Growth completed");
        ChangeSprite(fullyGrownSprite);
        OnMature();
        if (plantCollider != null)
        {
            plantCollider.enabled = true;
        }
    }

    protected virtual void OnMature()
    {
        if (lifeController != null)
        {
            float newMaxHealth = lifeController.maxHealth * 1.5f;
            lifeController.maxHealth = newMaxHealth;
            lifeController.currentHealth = newMaxHealth;
            lifeController.onHealthChanged?.Invoke(lifeController.currentHealth, lifeController.maxHealth);
            Debug.Log($"Plant matured: Health increased to {lifeController.maxHealth}");
        }
    }

    public bool IsFullyGrown()
    {
        return growthCompleted;
    }

    public float GetGrowthPercentage()
    {
        if (growthCompleted)
            return 1.0f;

        int currentDay = GameManager.Instance.GetCurrentDay();
        int daysSincePlanting = currentDay - plantingDay;

        if (daysSincePlanting <= 0)
            return 0f;

        return Mathf.Clamp01((float)daysSincePlanting / daysToGrow);
    }

    private void HandlePlantDeath()
    {
        TilePlantingSystem.Instance.UnregisterPlantAt(tilePosition);
        TributeSystem.Instance?.NotifyPlantDestroyed();
        Debug.Log("Plant has been destroyed!");
    }

    private void ChangeSprite(Sprite newSprite)
    {
        if (spriteRenderer != null && newSprite != null)
        {
            spriteRenderer.sprite = newSprite;
        }
    }

    public void TakeDamage(float damage)
    {
        if (lifeController != null)
        {
            lifeController.TakeDamage(damage);
        }
    }
}