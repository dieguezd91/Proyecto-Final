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

    [Header("Cumulative XP to enter L1, L2, ... (append explicit L11+ only)")]
    [SerializeField] private List<int> cumulativeThresholds = new List<int>
        { 0, 100, 250, 450, 750, 1200, 1800, 2600, 3500, 4500 };

    public IReadOnlyList<int> CumulativeThresholds => cumulativeThresholds;
    public int GetLevelThreshold(int level) => level > 0 && level <= cumulativeThresholds.Count ? cumulativeThresholds[level - 1] : 0;

    public int ExperiencePerNight => Mathf.Max(1, experiencePerNight);
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
        return MagicGarden.ExperienceCurve.Next(cumulativeThresholds, level);
    }
}
