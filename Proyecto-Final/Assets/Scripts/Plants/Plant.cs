using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Plant : MonoBehaviour
{
    [Header("Growth Configuration")]
    public int daysToGrow = 3;

    [Header("Plant Settings")]
    public float healthMultiplierWhenMature = 1.5f;

    [Header("State")]
    [SerializeField] private int plantingDay = 0;
    [SerializeField] private bool growthCompleted = false;

    private Vector3 initialScale;
    private Vector3 finalScale = Vector3.one;
    private LifeController lifeController;
    private PlantingSpot plantingSpot;

    protected virtual void Start()
    {
        initialScale = Vector3.one * 0.2f;
        transform.localScale = initialScale;

        plantingDay = GameManager.Instance.GetCurrentDay();

        GameManager.Instance.onNewDay.AddListener(OnNewDay);

        UpdateGrowthStatus(GameManager.Instance.GetCurrentDay());

        lifeController = GetComponent<LifeController>();
        if (lifeController == null)
        {
            lifeController = gameObject.AddComponent<LifeController>();
        }

        lifeController.onDeath.AddListener(HandlePlantDeath);

        FindPlantingSpot();
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

    private void OnNewDay(int currentDay)
    {
        UpdateGrowthStatus(currentDay);
    }

    private void UpdateGrowthStatus(int currentDay)
    {
        if (growthCompleted)
            return;

        int daysSincePlanting = currentDay - plantingDay;

        float progress = Mathf.Clamp01((float)daysSincePlanting / daysToGrow);

        transform.localScale = Vector3.Lerp(initialScale, finalScale, progress);

        if (daysSincePlanting >= daysToGrow)
        {
            CompleteGrowth();
        }

        Debug.Log($"Plant: Growth updated. Day {currentDay}, Days since planting: {daysSincePlanting}/{daysToGrow}, Progress: {(progress * 100):F0}%");
    }

    private void CompleteGrowth()
    {
        growthCompleted = true;
        transform.localScale = finalScale;
        Debug.Log("Plant: Growth completed");

        OnMature();
    }

    protected virtual void OnMature()
    {
        if (lifeController != null && healthMultiplierWhenMature > 1.0f)
        {
            float newMaxHealth = lifeController.maxHealth * healthMultiplierWhenMature;
            float healthPercentage = lifeController.currentHealth / lifeController.maxHealth;

            lifeController.maxHealth = newMaxHealth;
            lifeController.currentHealth = newMaxHealth * healthPercentage;

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

        return Mathf.Clamp01((float)daysSincePlanting / daysToGrow);
    }

    private void HandlePlantDeath()
    {
        if (plantingSpot != null)
        {
            plantingSpot.isOccupied = false;
        }

        Debug.Log("Plant has been destroyed!");
    }

    private void FindPlantingSpot()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.1f);
        foreach (Collider2D collider in colliders)
        {
            PlantingSpot spot = collider.GetComponent<PlantingSpot>();
            if (spot != null)
            {
                plantingSpot = spot;
                break;
            }
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