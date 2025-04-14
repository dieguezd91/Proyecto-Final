using System.Collections.Generic;
using UnityEngine;
using System;

public class LunarInfluenceManager : MonoBehaviour
{
    public static LunarInfluenceManager Instance { get; private set; }

    [Header("REFERENCES")]
    [SerializeField] private LunarCycleManager lunarCycleManager;
    [SerializeField] private ManaSystem manaSystem;

    [Header("MANA INFLUENCES")]
    [SerializeField] private float[] manaRegenModifiers = new float[5] { 2.0f, 1.15f, 1.0f, 0.9f, 0.5f };
    [SerializeField] private float[] maxManaModifiers = new float[5] { 0.9f, 1.0f, 1.0f, 1.1f, 1.2f };
    [SerializeField] private float[] manaCostModifiers = new float[5] { 1.0f, 1.0f, 1.0f, 0.9f, 1.0f };

    [Serializable]
    public class ElementalDamageModifiers
    {
        public float iceDamageModifier = 1.0f;
        public float windDamageModifier = 1.0f;
        public float electricDamageModifier = 1.0f;
        public float fireDamageModifier = 1.0f;
        public float stellarDamageModifier = 1.0f;
        public float lunarDamageModifier = 1.0f;
    }

    private MoonPhase currentMoonPhase;
    private Dictionary<MoonPhaseInfluence, float> currentInfluences = new Dictionary<MoonPhaseInfluence, float>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (lunarCycleManager == null)
        {
            lunarCycleManager = LunarCycleManager.Instance;
        }

        if (manaSystem == null)
        {
            manaSystem = FindObjectOfType<ManaSystem>();
        }

        if (lunarCycleManager != null)
        {
            lunarCycleManager.onMoonPhaseChanged.AddListener(OnMoonPhaseChanged);
            currentMoonPhase = lunarCycleManager.GetCurrentMoonPhase();
            UpdateLunarInfluences();
        }
    }

    private void OnMoonPhaseChanged(MoonPhase newPhase)
    {
        currentMoonPhase = newPhase;
        UpdateLunarInfluences();
    }

    private void UpdateLunarInfluences()
    {
        currentInfluences.Clear();
        int phaseIndex = (int)currentMoonPhase;

        if (phaseIndex >= 0 && phaseIndex < 5)
        {
            ApplyManaInfluences(phaseIndex);
        }
    }

    private void ApplyManaInfluences(int phaseIndex)
    {
        if (manaSystem != null)
        {
            if (phaseIndex >= 0 && phaseIndex < manaRegenModifiers.Length &&
                phaseIndex < maxManaModifiers.Length &&
                phaseIndex < manaCostModifiers.Length)
            {
                currentInfluences[MoonPhaseInfluence.ManaRegeneration] = manaRegenModifiers[phaseIndex];
                currentInfluences[MoonPhaseInfluence.MaxManaCapacity] = maxManaModifiers[phaseIndex];
                currentInfluences[MoonPhaseInfluence.ManaCost] = manaCostModifiers[phaseIndex];
            }
        }
    }

    public float GetManaRegenerationModifier()
    {
        return currentInfluences.TryGetValue(MoonPhaseInfluence.ManaRegeneration, out float value) ? value : 1.0f;
    }

    public float GetMaxManaModifier()
    {
        return currentInfluences.TryGetValue(MoonPhaseInfluence.MaxManaCapacity, out float value) ? value : 1.0f;
    }

    public float GetManaCostModifier()
    {
        return currentInfluences.TryGetValue(MoonPhaseInfluence.ManaCost, out float value) ? value : 1.0f;
    }
}

public enum MoonPhaseInfluence
{
    ManaRegeneration,
    MaxManaCapacity,
    ManaCost,
    PlantGrowth,
    EnemyStrength,
    ElementalDamage
}

public enum ElementType
{
    Ice,
    Wind,
    Electric,
    Fire,
    Stellar,
    Lunar
}