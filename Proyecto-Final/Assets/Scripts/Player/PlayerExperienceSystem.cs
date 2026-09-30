using System;
using UnityEngine;

public class PlayerExperienceSystem : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private ExperienceProgressionDataSO progressionData;

    [Header("Progression State")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExperience = 0;

    public int CurrentLevel => currentLevel;
    public int CurrentExperience => currentExperience;
    public int ExperienceRequired => GetRequiredExperience(currentLevel);
    public int ExperiencePerNight => progressionData != null ? progressionData.ExperiencePerNight : 0;
    public ExperienceProgressionDataSO ProgressionData => progressionData;

    public event Action<int, int, int> OnExperienceChanged; // level, currentExperience, experienceRequired
    public event Action<int> OnLevelUp;
    public event Action OnProgressionReset;

    private void Awake()
    {
        ValidateConfiguration();
        if (currentLevel < 1) currentLevel = 1;
        if (currentExperience < 0) currentExperience = 0;
    }

    private bool ValidateConfiguration()
    {
        if (progressionData == null)
        {
            Debug.LogError($"[PlayerExperienceSystem] Missing ExperienceProgressionDataSO reference on {gameObject.name}!", this);
            return false;
        }
        return true;
    }

    public int GetRequiredExperience(int level)
    {
        if (!ValidateConfiguration())
        {
            return 0;
        }

        return progressionData.GetExperienceRequiredForLevel(level);
    }

    public int CalculateExperienceRequired(int level)
    {
        return GetRequiredExperience(level);
    }

    public void GrantNightCompletionExperience()
    {
        if (!ValidateConfiguration()) return;
        AddExperience(progressionData.ExperiencePerNight);
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0) return;
        if (!ValidateConfiguration()) return;

        currentExperience += amount;
        int required = ExperienceRequired;

        while (required > 0 && currentExperience >= required)
        {
            currentExperience -= required;
            currentLevel++;
            OnLevelUp?.Invoke(currentLevel);
            required = ExperienceRequired;
        }

        OnExperienceChanged?.Invoke(currentLevel, currentExperience, required);
    }

    public void ResetProgression()
    {
        currentLevel = 1;
        currentExperience = 0;
        OnExperienceChanged?.Invoke(currentLevel, currentExperience, ExperienceRequired);
        OnProgressionReset?.Invoke();
    }
}
