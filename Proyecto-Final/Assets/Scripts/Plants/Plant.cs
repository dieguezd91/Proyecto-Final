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

    [Header("FX")]
    [SerializeField] private GameObject preMatureParticlesPrefab;
    private GameObject activeParticlesInstance;
    private bool particlesActivated = false;

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
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

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
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        if (lifeController != null)
        {
            lifeController.onDeath.RemoveListener(HandlePlantDeath);
        }
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

        if (!particlesActivated && daysSincePlanting == daysToGrow - 1)
        {
            ActivatePreMatureParticles();
        }

        if (daysSincePlanting >= daysToGrow)
        {
            CompleteGrowth();
            return;
        }

        float progress = Mathf.Clamp01((float)daysSincePlanting / daysToGrow);

        if (progress <= 0.33f)
            ChangeSprite(startingDaySprite);
        else if (progress <= 0.66f)
            ChangeSprite(middleDaySprite);
        else if (progress < 1.0f)
            ChangeSprite(lastDaySprite);
    }

    private void ActivatePreMatureParticles()
    {
        if (!particlesActivated && preMatureParticlesPrefab != null)
        {
            activeParticlesInstance = Instantiate(preMatureParticlesPrefab, transform);
            activeParticlesInstance.transform.localPosition = Vector3.zero;
            particlesActivated = true;
        }
    }

    private void DeactivatePreMatureParticles()
    {
        if (particlesActivated)
        {
            if (activeParticlesInstance != null)
            {
                Destroy(activeParticlesInstance);
                activeParticlesInstance = null;
            }
            particlesActivated = false;
        }
    }

    public void OnNewDay(int currentDay)
    {
        UpdateGrowthStatus(currentDay);
    }

    private void CompleteGrowth()
    {
        if (growthCompleted)
            return;

        growthCompleted = true;
        DeactivatePreMatureParticles();
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
        DeactivatePreMatureParticles();
        TilePlantingSystem.Instance.UnregisterPlantAt(tilePosition);
        TributeSystem.Instance?.NotifyPlantDestroyed();
        Debug.Log("Plant has been destroyed!");
    }

    protected virtual void HandleGameStateChanged(GameState newState)
    {
        if (newState == GameState.Night)
        {
            UpdateGrowthStatus(GameManager.Instance.GetCurrentDay());
        }
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