using System;
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

    [SerializeField] private PlantSoundBase _soundBase;

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

    protected Animator animator;

    protected PlayerAbilitySystem abilitySystem;

    private Collider2D plantCollider;

    public PlantSoundBase SoundBase => _soundBase;

    protected virtual void Awake()
    {
        if (_soundBase == null)
        {
            _soundBase = GetComponent<PlantSoundBase>();
        }
    }

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();

        plantCollider = GetComponent<Collider2D>();
        if (plantCollider != null)
        {
            plantCollider.enabled = false;
        }

        plantingDay = LevelManager.Instance.GetCurrentDay();
        LevelManager.Instance.OnGameStateChanged += HandleGameStateChanged;

        abilitySystem = FindObjectOfType<PlayerAbilitySystem>();

        if (plantData != null)
        {
            daysToGrow = plantData.daysToGrow;
            plantSprite = plantData.plantIcon;
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

        ConfigureLifeController();

        lifeController.onDeath.AddListener(HandlePlantDeath);
    }

    protected virtual void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
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

        if (!particlesActivated && daysSincePlanting == daysToGrow - 1)
        {
            ActivatePreMatureParticles();
        }

        if (daysSincePlanting > daysToGrow)
        {
            CompleteGrowth();
            return;
        }

        SetGrowthAnimationStage(daysSincePlanting);
    }

    private void SetGrowthAnimationStage(int daysSincePlanting)
    {
        float growthProgress = Mathf.Clamp01((float)daysSincePlanting / daysToGrow);
        float blendValue = growthProgress * 2f;

        if (animator != null)
        {
            animator.SetFloat("GrowthStage", blendValue);
        }
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

        if (animator != null)
        {
            animator.SetBool("GrowthCompleted", true);
            animator.SetFloat("GrowthStage", 2f);
        }

        OnMature();

        if (plantCollider != null)
            plantCollider.enabled = true;
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

        int currentDay = LevelManager.Instance.GetCurrentDay();
        int daysSincePlanting = currentDay - plantingDay;

        if (daysSincePlanting <= 0)
            return 0f;

        return Mathf.Clamp01((float)daysSincePlanting / daysToGrow);
    }

    private void HandlePlantDeath()
    {
        DeactivatePreMatureParticles();
        SoundBase.PlaySound(PlantSoundType.Die, SoundSourceType.Localized, transform);
        TilePlantingSystem.Instance.UnregisterPlantAt(tilePosition);
        RewardsSystem.Instance?.NotifyPlantDestroyed();
    }

    private void ConfigureLifeController()
    {
        bool hasAnimation = plantData != null && plantData.hasDeathAnimation;
        lifeController.ConfigureAsPlant(hasAnimation);
    }

    protected virtual void HandleGameStateChanged(GameState newState)
    {
        if (newState == GameState.Night)
        {
            UpdateGrowthStatus(LevelManager.Instance.GetCurrentDay());
        }
    }
}