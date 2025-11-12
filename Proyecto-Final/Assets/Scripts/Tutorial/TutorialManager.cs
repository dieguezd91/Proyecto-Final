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

    private TutorialStep currentStep;
    private int currentStepIndex = 0;
    private int currentProgress = 0;
    private bool tutorialActive = false;
    private bool isTransitioning = false;

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
        if (enableTutorial && tutorialSteps.Count > 0)
        {
            Invoke(nameof(StartTutorial), 0.5f);
        }

        playerController = FindObjectOfType<PlayerController>();
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

        int bufferSize = eventBuffer.Count;
        if (bufferSize > 0)
        {
            for (int i = 0; i < bufferSize; i++)
            {
                TutorialObjectiveType bufferedEvent = eventBuffer.Dequeue();

                if (bufferedEvent == currentStep.objectiveType)
                {
                    CheckObjective(bufferedEvent);
                    if (isTransitioning) return;
                }
                else
                {
                    eventBuffer.Enqueue(bufferedEvent);
                }
            }
        }

        if (tutorialUI != null)
        {
            if (!string.IsNullOrEmpty(currentStep.instructionText))
            {
                tutorialUI.ShowStep(currentStep);
            }
        }

        if (currentStep.objectiveType == TutorialObjectiveType.Wait && !isTransitioning)
        {
            float duration = currentStep.waitDuration > 0 ? currentStep.waitDuration : 3f;
            StartCoroutine(AutoCompleteWaitStep(duration));
        }
    }

    private IEnumerator AutoCompleteWaitStep(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentStep != null && currentStep.objectiveType == TutorialObjectiveType.Wait)
        {
            CompleteCurrentStep();
        }
    }

    private void CheckObjective(TutorialObjectiveType type)
    {
        if (currentStep != null && currentStep.objectiveType == TutorialObjectiveType.Wait)
        {
            if (type == TutorialObjectiveType.Move) return;

            if (!eventBuffer.Contains(type))
            {
                eventBuffer.Enqueue(type);
            }
            return;
        }

        if (isTransitioning)
        {
            if (!eventBuffer.Contains(type))
            {
                eventBuffer.Enqueue(type);
            }
            return;
        }

        if (!tutorialActive || currentStep == null)
        {
            return;
        }

        if (currentStep.objectiveType != type)
        {
            if (!eventBuffer.Contains(type))
            {
                eventBuffer.Enqueue(type);
            }
            return;
        }

        currentProgress++;

        if (currentProgress >= currentStep.requiredCount)
        {
            CompleteCurrentStep();
        }
    }

    private void CompleteCurrentStep()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (playerController != null)
        {
            playerController.SetMovementEnabled(true);
            playerController.SetCanAct(true);
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

    private void ShowNextStepDelayed()
    {
        ShowStep(currentStepIndex + 1);
    }

    private void CompleteTutorial()
    {
        tutorialActive = false;

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
    private void CheckObjective_HybridPlantPlanted() => CheckObjective(TutorialObjectiveType.PlantHybrid);
    private void CheckObjective_CraftingProximity() => CheckObjective(TutorialObjectiveType.CraftingProximity);
    private void CheckObjective_RestorationProximity() => CheckObjective(TutorialObjectiveType.RestorationProximity);
    private void CheckObjective_RitualAltarProximity() => CheckObjective(TutorialObjectiveType.RitualAltarProximity);
    private void CheckObjective_FirstPlantReady() => CheckObjective(TutorialObjectiveType.FirstPlantReady);
    private void CheckObjective_TeleportCasted() => CheckObjective(TutorialObjectiveType.TeleportSpell);

    public void SkipTutorial()
    {
        if (tutorialActive)
        {
            CompleteTutorial();
        }
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
}