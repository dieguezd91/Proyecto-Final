using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityUIManager : MonoBehaviour
{
    [Header("Ability Slots")]
    [SerializeField] private AbilitySlot[] abilitySlots = new AbilitySlot[4];

    private PlayerAbilitySystem playerAbilitySystem;

    void Start()
    {
        playerAbilitySystem = FindObjectOfType<PlayerAbilitySystem>();

        if (playerAbilitySystem == null)
        {
            Debug.LogError("PlayerAbilitySystem not found!");
            return;
        }

        playerAbilitySystem.OnAbilityChanged += OnAbilityChanged;

        InitializeAbilitySlots();
        UpdateAllSlotVisuals();
    }

    private void OnDestroy()
    {
        CleanupEventListeners();
    }

    private void InitializeAbilitySlots()
    {
        foreach (var slot in abilitySlots)
        {
            if (slot == null) continue;

            // Initialize the slot without overwriting its Inspector-configured ability type
            slot.Initialize();
            slot.OnAbilitySelected += OnAbilitySlotSelected;
        }
    }

    private void OnAbilitySlotSelected(PlayerAbility selectedAbility)
    {
        if (playerAbilitySystem != null)
        {
            playerAbilitySystem.SetAbility(selectedAbility);
        }
    }

    private void OnAbilityChanged(PlayerAbility newAbility)
    {
        UpdateAllSlotVisuals();
        UIManager.Instance?.InterfaceSounds?.PlaySound(InterfaceSoundType.OnAbilityChanged);
    }

    private void UpdateAllSlotVisuals()
    {
        bool isDaytime = LevelManager.Instance?.currentGameState != GameState.Night;
        PlayerAbility currentAbility = playerAbilitySystem?.CurrentAbility ?? PlayerAbility.Planting;

        Debug.Log($"[AbilityUIManager] UpdateAllSlotVisuals - CurrentAbility: {currentAbility}, IsDaytime: {isDaytime}");

        foreach (var slot in abilitySlots)
        {
            if (slot != null)
            {
                Debug.Log($"[AbilityUIManager] Updating slot with AbilityType: {slot.AbilityType}");
                slot.RefreshVisualState(currentAbility, isDaytime);
            }
        }
    }


    private void CleanupEventListeners()
    {
        if (playerAbilitySystem != null)
        {
            playerAbilitySystem.OnAbilityChanged -= OnAbilityChanged;
        }

        foreach (var slot in abilitySlots)
        {
            if (slot != null)
            {
                slot.OnAbilitySelected -= OnAbilitySlotSelected;
            }
        }
    }

    public AbilitySlot GetSlotForAbility(PlayerAbility ability)
    {
        foreach (var slot in abilitySlots)
        {
            if (slot != null && slot.AbilityType == ability)
            {
                return slot;
            }
        }
        return null;
    }

    public void RefreshSlotVisuals()
    {
        UpdateAllSlotVisuals();
    }
}