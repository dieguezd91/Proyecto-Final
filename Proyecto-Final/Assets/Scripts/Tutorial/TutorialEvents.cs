using System;

public static class TutorialEvents
{
    public static event Action OnTutorialStarted;
    public static event Action<TutorialStep> OnStepCompleted;
    public static event Action OnTutorialCompleted;

    public static event Action OnPlayerMoved;
    public static event Action OnGroundDug;
    public static event Action OnSeedPlanted;
    public static event Action OnPlantHarvested;
    public static event Action OnSpellCasted;
    public static event Action OnNightStarted;
    public static event Action OnEnemyDefeated;
    public static event Action OnNightSurvived;

    public static event Action OnInventoryOpened;
    public static event Action OnCraftingOpened;
    public static event Action OnRestorationOpened;
    public static event Action OnHouseEntered;
    public static event Action OnRitualAltarUsed;

    public static void InvokeTutorialStarted() => OnTutorialStarted?.Invoke();
    public static void InvokeStepCompleted(TutorialStep step) => OnStepCompleted?.Invoke(step);
    public static void InvokeTutorialCompleted() => OnTutorialCompleted?.Invoke();

    public static void InvokeProductionPlantPlanted() => OnProductionPlantPlanted?.Invoke();
    public static void InvokeDefensivePlantPlanted() => OnDefensivePlantPlanted?.Invoke();

    public static void InvokePlayerMoved() => OnPlayerMoved?.Invoke();
    public static void InvokeGroundDug() => OnGroundDug?.Invoke();
    public static void InvokePlantHarvested() => OnPlantHarvested?.Invoke();
    public static void InvokeSpellCasted() => OnSpellCasted?.Invoke();
    public static void InvokeNightStarted() => OnNightStarted?.Invoke();
    public static void InvokeEnemyDefeated() => OnEnemyDefeated?.Invoke();
    public static void InvokeNightSurvived() => OnNightSurvived?.Invoke();

    public static void InvokeInventoryOpened() => OnInventoryOpened?.Invoke();
    public static void InvokeCraftingOpened() => OnCraftingOpened?.Invoke();
    public static void InvokeRestorationOpened() => OnRestorationOpened?.Invoke();
    public static void InvokeHouseEntered() => OnHouseEntered?.Invoke();
    public static void InvokeRitualAltarUsed() => OnRitualAltarUsed?.Invoke();

    public static event Action OnProductionPlantPlanted;
    public static event Action OnDefensivePlantPlanted;
}