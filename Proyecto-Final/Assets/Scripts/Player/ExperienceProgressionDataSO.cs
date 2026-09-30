using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ExperienceLevelData
{
    [Min(1)]
    [SerializeField] private int experienceRequired = 100;

    [SerializeField] private List<PlantDataSO> plantUnlocks = new List<PlantDataSO>();
    [SerializeField] private List<SpellDataSO> spellUnlocks = new List<SpellDataSO>();

    public int ExperienceRequired => Mathf.Max(1, experienceRequired);
    public IReadOnlyList<PlantDataSO> PlantUnlocks => plantUnlocks ?? (IReadOnlyList<PlantDataSO>)Array.Empty<PlantDataSO>();
    public IReadOnlyList<SpellDataSO> SpellUnlocks => spellUnlocks ?? (IReadOnlyList<SpellDataSO>)Array.Empty<SpellDataSO>();

    public ExperienceLevelData() { }

    public ExperienceLevelData(int required, List<PlantDataSO> plants = null, List<SpellDataSO> spells = null)
    {
        experienceRequired = required;
        plantUnlocks = plants ?? new List<PlantDataSO>();
        spellUnlocks = spells ?? new List<SpellDataSO>();
    }
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

    public ExperienceLevelData GetLevelData(int level)
    {
        if (level < 1)
        {
            level = 1;
        }

        int index = level - 1;
        if (levelRequirements != null && index >= 0 && index < levelRequirements.Count)
        {
            return levelRequirements[index];
        }

        return null;
    }

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
            return Mathf.Max(1, levelRequirements[index].ExperienceRequired);
        }

        int lastConfiguredLevel = levelRequirements.Count;
        int lastRequired = Mathf.Max(
            1,
            levelRequirements[lastConfiguredLevel - 1].ExperienceRequired);

        int levelsAbove = level - lastConfiguredLevel;

        return lastRequired +
               levelsAbove * ExperienceIncreaseAfterLastLevel;
    }
}
