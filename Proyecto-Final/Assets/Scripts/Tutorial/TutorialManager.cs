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
    private bool _completeAfterTypingSubscribed = false;

    private bool isPausedByMenu = false;

    // Defer/hold the automatic showing of the next step after completing the current step.
    // Useful when the game needs to wait for an external event (e.g., ritual animation) before
    // showing the next tutorial instruction.
    private bool nextStepDeferred = false;
    private bool nextStepPending = false;

    private Queue<TutorialObjectiveType> eventBuffer = new Queue<TutorialObjectiveType>();
    private PlayerController playerController;

    public bool IsTutorialActive() => tutorialActive;
    
    private void ApplyInputGatingForStep(TutorialStep step)
    {
        if (playerController == null || step == null) return;

        if (step.objectiveType == TutorialObjectiveType.Move)
        {
            playerController.SetMovementEnabled(true);
            playerController.SetCanAct(true);
            return;
        }

        if (step.isGatedStep)
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
    
    private void ArmMoveTriggerIfNeeded(TutorialStep step)
    {
        if (step == null || playerController == null) return;

        if (step.objectiveType != TutorialObjectiveType.Move) return;

        playerController.ResetHasMovedForTutorial();
        if (playerController.IsCurrentlyMoving())
        {
            TutorialEvents.InvokePlayerMoved();
        }
    }
    
    private void PreApplyGatingForUpcomingStep()
    {
        int nextIndex = currentStepIndex + 1;
        if (tutorialSteps == null || nextIndex >= tutorialSteps.Count) return;

        var nextStep = tutorialSteps[nextIndex];
        if (nextStep == null) return;

        if (nextStep.isGatedStep)
        {
            if (playerController != null)
            {
                playerController.SetMovementEnabled(false);
                playerController.SetCanAct(false);
            }

            // Prevent accepting buffered input until the next step is shown
            canAcceptInput = false;
        }
    }
    
    private void DisplayStep(TutorialStep step, bool force = false)
    {
        if (step == null) return;

        ApplyInputGatingForStep(step);
        ArmMoveTriggerIfNeeded(step);

        if (tutorialUI != null)
        {
            if (force)
                tutorialUI.ForceShowStep(step);
            else
                tutorialUI.ShowStep(step);
        }

        // Remove stale movement events that might have occurred before the step became visible
        RemoveBufferedObjective(TutorialObjectiveType.Move);

        // Schedule processing of the event buffer after a small delay so UI/LevelManager states settle
        Invoke(nameof(ProcessBufferAndEnableInput), bufferProcessDelay);
    }

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

        // Pause/resume tutorial when inventory or pause menus open/close
        UIEvents.OnPauseMenuRequested += PauseTutorial;
        UIEvents.OnPauseMenuClosed += ResumeTutorial;
        UIEvents.OnInventoryOpened += PauseTutorial;
        UIEvents.OnInventoryClosed += ResumeTutorial;
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

        UIEvents.OnPauseMenuRequested -= PauseTutorial;
        UIEvents.OnPauseMenuClosed -= ResumeTutorial;
        UIEvents.OnInventoryOpened -= PauseTutorial;
        UIEvents.OnInventoryClosed -= ResumeTutorial;
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

        TutorialEvents.InvokeStepCompleted(currentStep);

        // Pre-apply gating for the upcoming step (if any) so the player cannot move
        // while UI panels/animations execute. This covers both immediate and deferred
        // transitions to the next tutorial step.
        PreApplyGatingForUpcomingStep();

        if (tutorialUI != null)
        {
            tutorialUI.HideStep();
            if (nextStepDeferred)
            {
                // Mark that a next step is pending, but don't show it yet.
                nextStepPending = true;
            }
            else
            {
                Invoke(nameof(ShowNextStepDelayed), 0.6f);
            }
        }
        else
        {
            if (nextStepDeferred)
            {
                nextStepPending = true;
            }
            else
            {
                ShowStep(currentStepIndex + 1);
            }
        }
    }

    // Call to defer showing the next step after the current step completes.
    public void DeferNextStep()
    {
        nextStepDeferred = true;
    }

    // Release a previously deferred next-step and show it if one was pending.
    public void ReleaseDeferredNextStep(bool immediate = false)
    {
        if (!nextStepDeferred && !nextStepPending) return;

        nextStepDeferred = false;

        if (!nextStepPending) return;

        nextStepPending = false;

        if (immediate)
        {
            // Show next step immediately (bypass the small transition delay)
            ShowNextStepDelayed(); // directly call (no invoke) to avoid the 0.6s wait
        }
        else
        {
            // Keep the small delay used when normally transitioning to the next step.
            Invoke(nameof(ShowNextStepDelayed), 0.6f);
        }
    }
    
    private void ShowStep(int index)
    {
        // Ensure we don't carry over any typing-finish subscription from previous step
        if (_completeAfterTypingSubscribed && tutorialUI != null)
        {
            tutorialUI.TypingFinished -= OnUITypingFinishedToCompleteStep;
            _completeAfterTypingSubscribed = false;
        }

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
        
        if (tutorialUI != null)
        {
            if (!string.IsNullOrEmpty(currentStep.instructionText))
            {
                var lm = LevelManager.Instance;
                bool inInventory = lm != null && lm.currentGameState == GameState.OnInventory;
                bool inPaused = lm != null && lm.currentGameState == GameState.Paused;

                if (inInventory || inPaused)
                {
                    isPausedByMenu = true;
                    UIEvents.OnInventoryClosed += ShowPendingStepFromMenu;
                    UIEvents.OnPauseMenuClosed += ShowPendingStepFromMenu;
                }
                else
                {
                    // Use centralized display path (applies gating and arms move trigger if needed)
                    DisplayStep(currentStep);
                }
            }
        }
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
                    // If the UI is still typing text, defer completing the step until typing finishes
                    if (tutorialUI != null && tutorialUI.IsTyping)
                    {
                        if (!_completeAfterTypingSubscribed)
                        {
                            tutorialUI.TypingFinished += OnUITypingFinishedToCompleteStep;
                            _completeAfterTypingSubscribed = true;
                        }
                        // leave remaining events buffered
                        foreach (var evt in remainingEvents)
                        {
                            eventBuffer.Enqueue(evt);
                        }
                        return;
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
        if (!tutorialActive || currentStep == null) return;

        if (currentStep.objectiveType == TutorialObjectiveType.Wait) return;

        if (currentStep.objectiveType != type) return;

        if (isTransitioning) return;

        if (!canAcceptInput)
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

    public void PauseTutorial()
    {
        if (!tutorialActive || isPausedByMenu) return;

        isPausedByMenu = true;

        if (tutorialUI != null)
        {
            tutorialUI.HideStepImmediate();
        }

        Debug.Log("[Tutorial] Tutorial pausado por menú");
    }

    public void ResumeTutorial()
    {
        if (!tutorialActive || !isPausedByMenu) return;

        isPausedByMenu = false;

        if (tutorialUI != null && currentStep != null)
        {
            // Apply gating and arm move trigger via helpers so behavior is consistent
            ApplyInputGatingForStep(currentStep);
            ArmMoveTriggerIfNeeded(currentStep);

            tutorialUI.ForceShowStep(currentStep);
            Invoke(nameof(ProcessBufferAndEnableInput), bufferProcessDelay);
        }

        Debug.Log("[Tutorial] Tutorial reanudado");
    }

    private void ShowPendingStepFromMenu()
    {
        // Unsubscribe both in case either fires
        UIEvents.OnInventoryClosed -= ShowPendingStepFromMenu;
        UIEvents.OnPauseMenuClosed -= ShowPendingStepFromMenu;

        isPausedByMenu = false;

        Debug.LogFormat("TutorialManager: ShowPendingStepFromMenu called for step {0}", currentStepIndex);

        if (tutorialUI != null && currentStep != null && !string.IsNullOrEmpty(currentStep.instructionText))
        {
            // Defer slightly so the UI and LevelManager state can settle and avoid immediate hide races
            Debug.LogFormat("TutorialManager: scheduling ForceShowStep for step {0} (delayed)", currentStepIndex);
            Invoke(nameof(DelayedForceShowCurrentStep), 0.05f);
        }
    }

    private void DelayedForceShowCurrentStep()
    {
        if (tutorialUI != null && currentStep != null && !string.IsNullOrEmpty(currentStep.instructionText))
        {

            // If this is a Move tutorial, re-arm the player's move trigger and trigger immediately if moving
            if (currentStep.objectiveType == TutorialObjectiveType.Move && playerController != null)
            {
                playerController.ResetHasMovedForTutorial();
                if (playerController.IsCurrentlyMoving()) TutorialEvents.InvokePlayerMoved();
            }

            tutorialUI.ForceShowStep(currentStep);
            RemoveBufferedObjective(TutorialObjectiveType.Move);
            Invoke(nameof(ProcessBufferAndEnableInput), bufferProcessDelay);
        }
    }
    
    private void RemoveBufferedObjective(TutorialObjectiveType type)
    {
        if (eventBuffer == null || eventBuffer.Count == 0) return;
        var newQueue = new Queue<TutorialObjectiveType>();
        while (eventBuffer.Count > 0)
        {
            var evt = eventBuffer.Dequeue();
            if (evt != type) newQueue.Enqueue(evt);
        }
        eventBuffer = newQueue;
    }

    private void Update()
    {
        if (!isPausedByMenu) return;

        var lm = LevelManager.Instance;
        if (lm == null) return;

        // If we were paused by a menu but the level state no longer indicates pause/inventory, resume and show the pending step
        if (lm.currentGameState != GameState.Paused && lm.currentGameState != GameState.OnInventory)
        {
            ShowPendingStepFromMenu();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            CompleteCurrentStep();
        }
    }

    private void OnUITypingFinishedToCompleteStep()
    {
        // Unsubscribe to avoid repeated triggers
        if (tutorialUI != null) tutorialUI.TypingFinished -= OnUITypingFinishedToCompleteStep;
        _completeAfterTypingSubscribed = false;

        // Delay a bit to allow the player to read final characters
        StartCoroutine(DelayedCompleteAfterTypingCoroutine());
    }

    private IEnumerator DelayedCompleteAfterTypingCoroutine()
    {
        yield return new WaitForSeconds(1f);
        if (!isTransitioning)
        {
            CompleteCurrentStep();
        }
    }
}
