using System;

public static class UIEvents
{
    // Health Events
    public static event Action<float, float> OnPlayerHealthChanged;
    public static event Action<float, float> OnHomeHealthChanged;

    // Mana Events
    public static event Action OnManaChanged;

    // Inventory Events
    public static event Action OnInventoryToggleRequested;
    public static event Action OnInventoryOpened;
    public static event Action OnInventoryClosed;

    // Game State Events
    public static event Action<GameState> OnGameStateChanged;
    public static event Action OnInstructionsRequested;

    // Seed Slots Events
    public static event Action<int> OnSlotSelected;
    public static event Action OnSeedCountsUpdated;

    // Feedback Events
    public static event Action<float, UnityEngine.Vector3> OnPlayerDamaged;
    public static event Action<bool> OnGrayscaleRequested;

    // Tooltip Events
    public static event Action<int> OnTooltipRequested;
    public static event Action OnTooltipHideRequested;

    public static event Action<PlayerAbility> OnAbilityTooltipRequested;
    public static event Action OnAbilityTooltipHideRequested;

    public static event Action OnCraftingUIToggleRequested;
    public static event Action OnRestorationAltarUIToggleRequested;
    public static event Action OnCraftingUIClosed;
    public static event Action OnRestorationAltarUIClosed;

    public static void TriggerAbilityTooltipRequested(PlayerAbility ability)
        => OnAbilityTooltipRequested?.Invoke(ability);

    public static void TriggerAbilityTooltipHide()
        => OnAbilityTooltipHideRequested?.Invoke();

    public static void TriggerPlayerHealthChanged(float current, float max)
        => OnPlayerHealthChanged?.Invoke(current, max);

    public static void TriggerHomeHealthChanged(float current, float max)
        => OnHomeHealthChanged?.Invoke(current, max);

    public static void TriggerManaChanged()
        => OnManaChanged?.Invoke();

    public static void TriggerInventoryToggle()
        => OnInventoryToggleRequested?.Invoke();

    public static void TriggerInventoryOpened()
        => OnInventoryOpened?.Invoke();

    public static void TriggerInventoryClosed()
        => OnInventoryClosed?.Invoke();

    public static void TriggerGameStateChanged(GameState newState)
        => OnGameStateChanged?.Invoke(newState);

    public static void TriggerInstructionsRequested()
        => OnInstructionsRequested?.Invoke();

    public static void TriggerPlayerDamaged(float damage, UnityEngine.Vector3 position)
        => OnPlayerDamaged?.Invoke(damage, position);

    public static void TriggerSlotSelected(int slotIndex)
        => OnSlotSelected?.Invoke(slotIndex);

    public static void TriggerSeedCountsUpdated()
        => OnSeedCountsUpdated?.Invoke();

    public static void TriggerTooltipRequested(int slotIndex)
        => OnTooltipRequested?.Invoke(slotIndex);

    public static void TriggerTooltipHide()
        => OnTooltipHideRequested?.Invoke();

    public static void TriggerGrayscaleRequested(bool enabled)
        => OnGrayscaleRequested?.Invoke(enabled);

    public static void TriggerCraftingUIToggle()
    => OnCraftingUIToggleRequested?.Invoke();

    public static void TriggerRestorationAltarUIToggle()
    => OnRestorationAltarUIToggleRequested?.Invoke();

    public static void TriggerCraftingUIClosed()
    => OnCraftingUIClosed?.Invoke();

    public static void TriggerRestorationAltarUIClosed()
        => OnRestorationAltarUIClosed?.Invoke();
}