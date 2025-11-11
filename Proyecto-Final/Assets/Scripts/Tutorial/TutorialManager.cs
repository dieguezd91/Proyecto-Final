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
        TutorialEvents.OnSeedPlanted += CheckObjective_SeedPlanted;
        TutorialEvents.OnPlantHarvested += CheckObjective_PlantHarvested;
        TutorialEvents.OnSpellCasted += CheckObjective_SpellCasted;
        TutorialEvents.OnNightStarted += CheckObjective_NightStarted;
        TutorialEvents.OnEnemyDefeated += CheckObjective_EnemyDefeated;
        TutorialEvents.OnNightSurvived += CheckObjective_NightSurvived;

        TutorialEvents.OnHouseEntered += CheckObjective_HouseEntered;
        TutorialEvents.OnCraftingOpened += CheckObjective_CraftingOpened;
        TutorialEvents.OnRestorationOpened += CheckObjective_RestorationOpened;
        TutorialEvents.OnRitualAltarUsed += CheckObjective_RitualAltarUsed;
    }

    private void UnsubscribeFromEvents()
    {
        TutorialEvents.OnPlayerMoved -= CheckObjective_PlayerMoved;
        TutorialEvents.OnGroundDug -= CheckObjective_GroundDug;
        TutorialEvents.OnSeedPlanted -= CheckObjective_SeedPlanted;
        TutorialEvents.OnPlantHarvested -= CheckObjective_PlantHarvested;
        TutorialEvents.OnSpellCasted -= CheckObjective_SpellCasted;
        TutorialEvents.OnNightStarted -= CheckObjective_NightStarted;
        TutorialEvents.OnEnemyDefeated -= CheckObjective_EnemyDefeated;
        TutorialEvents.OnNightSurvived -= CheckObjective_NightSurvived;

        TutorialEvents.OnHouseEntered -= CheckObjective_HouseEntered;
        TutorialEvents.OnCraftingOpened -= CheckObjective_CraftingOpened;
        TutorialEvents.OnRestorationOpened -= CheckObjective_RestorationOpened;
        TutorialEvents.OnRitualAltarUsed -= CheckObjective_RitualAltarUsed;
    }


    public void StartTutorial()
    {
        if (tutorialSteps == null || tutorialSteps.Count == 0)
        {
            return;
        }

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

        if (tutorialUI != null)
        {
            tutorialUI.ShowStep(currentStep);
        }

        if (currentStep.objectiveType == TutorialObjectiveType.Wait)
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
        if (!tutorialActive || currentStep == null)
        {
            Debug.Log($"[Tutorial] Evento {type} ignorado - Tutorial no activo o sin step");
            return;
        }

        if (currentStep.objectiveType != type)
        {
            Debug.Log($"[Tutorial] Evento {type} ignorado - Step actual requiere {currentStep.objectiveType}");
            return;
        }

        currentProgress++;

        Debug.Log($"[Tutorial] ✓ Progreso {type}: {currentProgress}/{currentStep.requiredCount}");

        if (currentProgress >= currentStep.requiredCount)
        {
            Debug.Log($"[Tutorial] ¡Step completado! Avanzando...");
            CompleteCurrentStep();
        }
    }

    private void CompleteCurrentStep()
    {
        TutorialEvents.InvokeStepCompleted(currentStep);

        Invoke(nameof(ShowNextStep), 1.5f);
    }

    private void ShowNextStep()
    {
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

    public bool IsTutorialActive() => tutorialActive;

    public void SkipTutorial()
    {
        if (tutorialActive)
        {
            CompleteTutorial();
        }
    }
}