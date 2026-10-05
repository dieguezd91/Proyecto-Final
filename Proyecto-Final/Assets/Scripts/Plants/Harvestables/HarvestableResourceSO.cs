using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HarvestLootEntry
{
    [Tooltip("Pickup prefab to spawn (e.g. Prefabs/Enemy Materials/Lunar Essence). Each harvest spawns exactly ONE pickup.")]
    public GameObject pickupPrefab;

    [Tooltip("Relative weight. Higher = more likely. Two entries with weight 1 are 50% / 50%.")]
    [Min(0f)] public float weight = 1f;

    [Tooltip("Calculated automatically from the weights. Read only.")]
    [SerializeField] private string chance;

    public void SetChanceLabel(float totalWeight)
    {
        chance = totalWeight > 0f ? $"{weight / totalWeight * 100f:0.#}%" : "0%";
    }
}

/// <summary>
/// Data for a harvestable world resource (plant, crystal, bush...).
/// Create one per resource type: Create > Crafting > Harvestable Resource Data.
/// </summary>
[CreateAssetMenu(fileName = "New Harvestable Resource Data", menuName = "Crafting/Harvestable Resource Data")]
public class HarvestableResourceSO : ScriptableObject
{
    [Header("IDENTITY")]
    [SerializeField] private string displayName = "New Harvestable";
    [TextArea(2, 4)]
    [SerializeField] private string description;

    [Header("HARVESTING")]
    [Tooltip("How many times it can be harvested per day. Refills when a new day starts.")]
    [SerializeField, Min(1)] private int maxUsesPerDay = 3;

    [Tooltip("Radius (world units) of the range trigger circle. The player must stay inside it to start or keep harvesting.")]
    [SerializeField, Min(0.1f)] private float interactionRange = 2f;

    [Tooltip("Seconds the player has to channel the harvest tool for one use.")]
    [SerializeField, Min(0f)] private float harvestDuration = 1.5f;

    [Header("LOOT (1 random pickup per harvest)")]
    [SerializeField] private List<HarvestLootEntry> lootTable = new List<HarvestLootEntry>();

    [Tooltip("Push applied to the spawned pickup so it pops out of the object.")]
    [SerializeField, Min(0f)] private float lootScatterForce = 3f;

    [Header("ANIMATION")]
    [Tooltip("Animator Override Controller based on 'Harvestable Base'. Duplicate one, then swap its Idle, Harvesting, Harvested, Depleted and Regrow clips.")]
    [SerializeField] private RuntimeAnimatorController animatorController;

    [Header("PARTICLES (prefabs, optional)")]
    [Tooltip("Spawned when channeling starts, removed when it ends.")]
    [SerializeField] private GameObject harvestingParticles;
    [Tooltip("Spawned after each successful harvest.")]
    [SerializeField] private GameObject harvestedParticles;
    [Tooltip("Spawned when the last use is spent.")]
    [SerializeField] private GameObject depletedParticles;
    [Tooltip("Spawned when uses refill at the start of a new day.")]
    [SerializeField] private GameObject regrowParticles;
    [Tooltip("Seconds before one-shot particles are destroyed.")]
    [SerializeField, Min(0.1f)] private float particleLifetime = 3f;

    [Header("SOUNDS (optional)")]
    [SerializeField] private SoundClipData harvestingSound = new SoundClipData();
    [SerializeField] private SoundClipData harvestedSound = new SoundClipData();
    [SerializeField] private SoundClipData depletedSound = new SoundClipData();
    [SerializeField] private SoundClipData regrowSound = new SoundClipData();

    [Header("HIGHLIGHT")]
    [Tooltip("Tint when hovered with the harvest tool inside range.")]
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.75f, 1f);
    [Tooltip("Tint while being harvested.")]
    [SerializeField] private Color harvestingColor = new Color(0.85f, 1f, 0.85f, 1f);

    public string DisplayName => displayName;
    public string Description => description;
    public int MaxUsesPerDay => maxUsesPerDay;
    public float InteractionRange => interactionRange;
    public float HarvestDuration => harvestDuration;
    public IReadOnlyList<HarvestLootEntry> LootTable => lootTable;
    public float LootScatterForce => lootScatterForce;

    public RuntimeAnimatorController AnimatorController => animatorController;

    public GameObject HarvestingParticles => harvestingParticles;
    public GameObject HarvestedParticles => harvestedParticles;
    public GameObject DepletedParticles => depletedParticles;
    public GameObject RegrowParticles => regrowParticles;
    public float ParticleLifetime => particleLifetime;

    public SoundClipData HarvestingSound => harvestingSound;
    public SoundClipData HarvestedSound => harvestedSound;
    public SoundClipData DepletedSound => depletedSound;
    public SoundClipData RegrowSound => regrowSound;

    public Color HighlightColor => highlightColor;
    public Color HarvestingColor => harvestingColor;

    /// <summary>Rolls one pickup prefab from the loot table by weight. Null if the table is empty.</summary>
    public GameObject RollLoot()
    {
        var roulette = new WeightedRoulette<GameObject>();

        foreach (var entry in lootTable)
        {
            if (entry != null && entry.pickupPrefab != null)
                roulette.Add(entry.pickupPrefab, entry.weight);
        }

        try
        {
            return roulette.Roll();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private void OnValidate()
    {
        float total = 0f;

        foreach (var entry in lootTable)
        {
            if (entry != null && entry.pickupPrefab != null)
                total += entry.weight;
        }

        foreach (var entry in lootTable)
            entry?.SetChanceLabel(entry.pickupPrefab != null ? total : 0f);
    }
}
