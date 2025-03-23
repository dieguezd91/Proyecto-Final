using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    [Header("Growth Configuration")]
    public int daysToGrow = 3;

    [Header("State")]
    [SerializeField] private int plantingDay = 0;
    [SerializeField] private bool growthCompleted = false;

    private Vector3 initialScale;
    private Vector3 finalScale = Vector3.one;

    protected virtual void Start()
    {
        initialScale = Vector3.one * 0.2f;
        transform.localScale = initialScale;

        plantingDay = GameManager.Instance.GetCurrentDay();

        GameManager.Instance.onNewDay.AddListener(OnNewDay);

        UpdateGrowthStatus(GameManager.Instance.GetCurrentDay());
    }

    protected virtual void Update()
    {

    }

    protected virtual void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onNewDay.RemoveListener(OnNewDay);
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
}