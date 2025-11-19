using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("SETTINGS")]
    [SerializeField] private List<TutorialStep> tutorialSteps;
    [SerializeField] private TutorialUI tutorialUI;
    [SerializeField] private bool enableTutorial = true;
    [SerializeField] private float bufferProcessDelay = 0.3f;

    private TutorialStep currentStep;
    private int currentStepIndex = 0;
    private int currentProgress = 0;
    private bool tutorialActive = false;
    private bool isTransitioning = false;
    private bool canAcceptInput = false;

    private Queue<TutorialObjectiveType> eventBuffer = new Queue<TutorialObjectiveType>();
    private PlayerController playerController;

    public bool IsTutorialActive() => tutorialActive;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();

        if (enableTutorial && tutorialSteps.Count > 0)
        {
            if (playerController != null)
            {
                playerController.SetMovementEnabled(false);
                playerController.SetCanAct(false);
            }

            Invoke(nameof(StartTutorial), 0.5f);
        }
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        TutorialEvents.OnPlayerMoved += CheckObjective_PlayerMoved;
        TutorialEvents.OnGroundDug += CheckObjective_GroundDug;
        TutorialEvents.OnPlantHarvested += CheckObjective_PlantHarvested;
        TutorialEvents.OnSpellCasted += CheckObjective_SpellCasted;
        TutorialEvents.OnNightStarted += CheckObjective_NightStarted;
        TutorialEvents.OnEnemyDefeated += CheckObjective_EnemyDefeated;
        TutorialEvents.OnNightSurvived += CheckObjective_NightSurvived;

        TutorialEvents.OnInventoryOpened += CheckObjective_InventoryOpened;
        TutorialEvents.OnHouseEntered += CheckObjective_HouseEntered;
        TutorialEvents.OnCraftingClosed += CheckObjective_CraftingOpened;
        TutorialEvents.OnRestorationClosed += CheckObjective_RestorationOpened;
        TutorialEvents.OnRitualAltarUsed += CheckObjective_RitualAltarUsed;

        TutorialEvents.OnProductionPlantPlanted += CheckObjective_ProductionPlantPlanted;
        TutorialEvents.OnProductionPlantPlanted += CheckObjective_SeedPlanted;

        TutorialEvents.OnDefensivePlantPlanted += CheckObjective_DefensivePlantPlanted;
        TutorialEvents.OnDefensivePlantPlanted += CheckObjective_SeedPlanted;

        TutorialEvents.OnCraftingProximity += CheckObjective_CraftingProximity;
        TutorialEvents.OnRestorationProximity += CheckObjective_RestorationProximity;
        TutorialEvents.OnRitualAltarProximity += CheckObjective_RitualAltarProximity;

        TutorialEvents.OnFirstPlantReadyToHarvest += CheckObjective_FirstPlantReady;
        TutorialEvents.OnAbilityChanged += CheckObjective_AbilityChanged;
        TutorialEvents.OnTeleportCasted += CheckObjective_TeleportCasted;
    }

    private void UnsubscribeFromEvents()
    {
        TutorialEvents.OnPlayerMoved -= CheckObjective_PlayerMoved;
        TutorialEvents.OnGroundDug -= CheckObjective_GroundDug;
        TutorialEvents.OnPlantHarvested -= CheckObjective_PlantHarvested;
        TutorialEvents.OnSpellCasted -= CheckObjective_SpellCasted;
        TutorialEvents.OnNightStarted -= CheckObjective_NightStarted;
        TutorialEvents.OnEnemyDefeated -= CheckObjective_EnemyDefeated;
        TutorialEvents.OnNightSurvived -= CheckObjective_NightSurvived;

        TutorialEvents.OnInventoryOpened -= CheckObjective_InventoryOpened;
        TutorialEvents.OnHouseEntered -= CheckObjective_HouseEntered;
        TutorialEvents.OnCraftingClosed -= CheckObjective_CraftingOpened;
        TutorialEvents.OnRestorationClosed -= CheckObjective_RestorationOpened;
        TutorialEvents.OnRitualAltarUsed -= CheckObjective_RitualAltarUsed;

        TutorialEvents.OnProductionPlantPlanted -= CheckObjective_ProductionPlantPlanted;
        TutorialEvents.OnProductionPlantPlanted -= CheckObjective_SeedPlanted;

        TutorialEvents.OnDefensivePlantPlanted -= CheckObjective_DefensivePlantPlanted;
        TutorialEvents.OnDefensivePlantPlanted -= CheckObjective_SeedPlanted;

        TutorialEvents.OnCraftingProximity -= CheckObjective_CraftingProximity;
        TutorialEvents.OnRestorationProximity -= CheckObjective_RestorationProximity;
        TutorialEvents.OnRitualAltarProximity -= CheckObjective_RitualAltarProximity;

        TutorialEvents.OnFirstPlantReadyToHarvest -= CheckObjective_FirstPlantReady;
        TutorialEvents.OnAbilityChanged -= CheckObjective_AbilityChanged;
        TutorialEvents.OnTeleportCasted -= CheckObjective_TeleportCasted;
    }

    public void StartTutorial()
    {
        if (tutorialSteps == null || tutorialSteps.Count == 0)
        {
            return;
        }

        TutorialEvents.ResetTutorialEventFlags();

        tutorialActive = true;
        tutorialSteps = tutorialSteps.OrderBy(s => s.stepOrder).ToList();

        TutorialEvents.InvokeTutorialStarted();
        ShowStep(0);
    }

    private void CompleteCurrentStep()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        canAcceptInput = false;

        if (playerController != null)
        {
            playerController.SetMovementEnabled(false);
            playerController.SetCanAct(false);
        }

        TutorialEvents.InvokeStepCompleted(currentStep);

        if (tutorialUI != null)
        {
            tutorialUI.HideStep();
            Invoke(nameof(ShowNextStepDelayed), 0.6f);
        }
        else
        {
            ShowStep(currentStepIndex + 1);
        }
    }

    private void ShowStep(int index)
    {
        if (index >= tutorialSteps.Count)
        {
            CompleteTutorial();
            return;
        }

        currentStepIndex = index;
        currentStep = tutorialSteps[index];
        currentProgress = 0;
        isTransitioning = false;
        canAcceptInput = false;

        if (playerController != null)
        {
            if (currentStep.objectiveType == TutorialObjectiveType.Wait)
            {
                playerController.SetMovementEnabled(false);
                playerController.SetCanAct(false);
            }
            else
            {
                playerController.SetMovementEnabled(true);
                playerController.SetCanAct(true);
            }
        }

        if (tutorialUI != null)
        {
            if (!string.IsNullOrEmpty(currentStep.instructionText))
            {
                tutorialUI.ShowStep(currentStep);
            }
        }

        Invoke(nameof(ProcessBufferAndEnableInput), bufferProcessDelay);
    }

    private void ProcessBufferAndEnableInput()
    {
        canAcceptInput = true;

        List<TutorialObjectiveType> remainingEvents = new List<TutorialObjectiveType>();

        while (eventBuffer.Count > 0)
        {
            TutorialObjectiveType bufferedEvent = eventBuffer.Dequeue();

            if (bufferedEvent == currentStep.objectiveType)
            {
                currentProgress++;

                if (currentProgress >= currentStep.requiredCount)
                {
                    while (eventBuffer.Count > 0)
                    {
                        remainingEvents.Add(eventBuffer.Dequeue());
                    }

                    foreach (var evt in remainingEvents)
                    {
                        eventBuffer.Enqueue(evt);
                    }

                    CompleteCurrentStep();
                    return;
                }
            }
            else
            {
                remainingEvents.Add(bufferedEvent);
            }
        }

        foreach (var evt in remainingEvents)
        {
            eventBuffer.Enqueue(evt);
        }
    }

    public void ConfirmWaitStep()
    {
        if (tutorialActive && !isTransitioning && currentStep != null && currentStep.objectiveType == TutorialObjectiveType.Wait)
        {
            CompleteCurrentStep();
        }
    }

    private void CheckObjective(TutorialObjectiveType type)
    {
        if (currentStep != null && currentStep.objectiveType == TutorialObjectiveType.Wait)
        {
            if (type == TutorialObjectiveType.Move) return;
            eventBuffer.Enqueue(type);
            return;
        }

        if (isTransitioning || !canAcceptInput)
        {
            eventBuffer.Enqueue(type);
            return;
        }

        if (!tutorialActive || currentStep == null)
        {
            return;
        }

        if (currentStep.objectiveType != type)
        {
            eventBuffer.Enqueue(type);
            return;
        }

        currentProgress++;

        if (currentProgress >= currentStep.requiredCount)
        {
            CompleteCurrentStep();
        }
    }

    private void ShowNextStepDelayed()
    {
        ShowStep(currentStepIndex + 1);
    }

    private void CompleteTutorial()
    {
        tutorialActive = false;
        canAcceptInput = false;

        if (playerController != null)
        {
            playerController.SetMovementEnabled(true);
            playerController.SetCanAct(true);
        }

        if (tutorialUI != null)
        {
            tutorialUI.HideStep();
        }

        TutorialEvents.InvokeTutorialCompleted();

        Debug.Log("Tutorial completado");
    }

    private void CheckObjective_PlayerMoved() => CheckObjective(TutorialObjectiveType.Move);
    private void CheckObjective_GroundDug() => CheckObjective(TutorialObjectiveType.Dig);
    private void CheckObjective_SeedPlanted() => CheckObjective(TutorialObjectiveType.Plant);
    private void CheckObjective_PlantHarvested() => CheckObjective(TutorialObjectiveType.Harvest);
    private void CheckObjective_SpellCasted() => CheckObjective(TutorialObjectiveType.CastSpell);
    private void CheckObjective_NightStarted() => CheckObjective(TutorialObjectiveType.StartNight);
    private void CheckObjective_EnemyDefeated() => CheckObjective(TutorialObjectiveType.DefeatEnemy);
    private void CheckObjective_NightSurvived() => CheckObjective(TutorialObjectiveType.SurviveNight);
    private void CheckObjective_HouseEntered() => CheckObjective(TutorialObjectiveType.EnterHouse);
    private void CheckObjective_CraftingOpened() => CheckObjective(TutorialObjectiveType.OpenCrafting);
    private void CheckObjective_RestorationOpened() => CheckObjective(TutorialObjectiveType.OpenRestoration);
    private void CheckObjective_RitualAltarUsed() => CheckObjective(TutorialObjectiveType.UseRitualAltar);
    private void CheckObjective_ProductionPlantPlanted() => CheckObjective(TutorialObjectiveType.PlantProduction);
    private void CheckObjective_DefensivePlantPlanted() => CheckObjective(TutorialObjectiveType.PlantDefensive);
    private void CheckObjective_CraftingProximity() => CheckObjective(TutorialObjectiveType.CraftingProximity);
    private void CheckObjective_RestorationProximity() => CheckObjective(TutorialObjectiveType.RestorationProximity);
    private void CheckObjective_RitualAltarProximity() => CheckObjective(TutorialObjectiveType.RitualAltarProximity);
    private void CheckObjective_FirstPlantReady() => CheckObjective(TutorialObjectiveType.FirstPlantReady);
    private void CheckObjective_TeleportCasted() => CheckObjective(TutorialObjectiveType.TeleportSpell);
    private void CheckObjective_InventoryOpened() => CheckObjective(TutorialObjectiveType.OpenInventory);
    private void CheckObjective_AbilityChanged() => CheckObjective(TutorialObjectiveType.AbilityChanged);

    public void SkipTutorial()
    {
        if (!tutorialActive) return;

        Debug.Log("Saltando tutorial...");

        StopAllCoroutines();
        CancelInvoke();

        isTransitioning = false;
        canAcceptInput = false;
        eventBuffer.Clear();
        currentStep = null;

        if (playerController != null)
        {
            playerController.SetMovementEnabled(true);
            playerController.SetCanAct(true);
        }

        CompleteTutorial();
    }

    public TutorialObjectiveType GetCurrentObjectiveType()
    {
        if (!tutorialActive || currentStep == null)
        {
            return TutorialObjectiveType.None;
        }

        return currentStep.objectiveType;
    }

    public int GetCurrentStepOrder()
    {
        if (!tutorialActive || currentStep == null)
        {
            return 9999;
        }
        return currentStep.stepOrder;
    }

    public bool IsPlayerGated()
    {
        if (!tutorialActive || currentStep == null)
        {
            return false;
        }

        return currentStep.isGatedStep;
    }
}