using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ExperienceLevelData
{
    [Min(1)]
    public int experienceRequired;
}

[CreateAssetMenu(fileName = "Experience Progression", menuName = "Game Data/Experience Progression")]
public class ExperienceProgressionDataSO : ScriptableObject
{
    [Header("Night Rewards")]
    [Min(1)]
    [SerializeField] private int experiencePerNight = 25;

    [Header("Level Curve Configuration")]
    [SerializeField] private List<ExperienceLevelData> levelRequirements = new List<ExperienceLevelData>();

    [Header("Extrapolation After Configured Levels")]
    [Min(1)]
    [SerializeField] private int experienceIncreaseAfterLastLevel = 50;

    public int ExperiencePerNight => Mathf.Max(1, experiencePerNight);
    public int ExperienceIncreaseAfterLastLevel => Mathf.Max(1, experienceIncreaseAfterLastLevel);
    public IReadOnlyList<ExperienceLevelData> LevelRequirements => levelRequirements;

    public int GetExperienceRequiredForLevel(int level)
    {
        if (level < 1)
        {
            level = 1;
        }

        if (levelRequirements == null || levelRequirements.Count == 0)
        {
            Debug.LogError(
                $"[{nameof(ExperienceProgressionDataSO)}] No level requirements configured.",
                this);

            return 0;
        }

        int index = level - 1;

        if (index < levelRequirements.Count)
        {
            return Mathf.Max(1, levelRequirements[index].experienceRequired);
        }

        int lastConfiguredLevel = levelRequirements.Count;
        int lastRequired = Mathf.Max(
            1,
            levelRequirements[lastConfiguredLevel - 1].experienceRequired);

        int levelsAbove = level - lastConfiguredLevel;

        return lastRequired +
               levelsAbove * ExperienceIncreaseAfterLastLevel;
    }
}
