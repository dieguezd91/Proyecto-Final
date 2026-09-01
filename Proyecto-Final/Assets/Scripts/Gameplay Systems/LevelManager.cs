using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ElementEnum
{
    Ice,
    Wind,
    Electric,
    Fire,
    Stellar,
    Lunar
}

public class LevelManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemiesSpawner waveSpawner;
    [SerializeField] private List<SpawnPointAnimator> spawnpoints = new List<SpawnPointAnimator>();
    [SerializeField] private AmbienceSoundManager ambienceSoundManager;
    
    public AmbienceSoundManager AmbienceSoundManager => ambienceSoundManager;

    public static LevelManager Instance { get; private set; }

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
        if (GameFlowController.Instance != null) GameFlowController.Instance.OnPhaseChanged += HandlePhaseChanged;

        if (waveSpawner == null)
        {
            waveSpawner = FindObjectOfType<EnemiesSpawner>();
            if (waveSpawner != null)
                waveSpawner.onHordeEnd.AddListener(HandleHordeCompleted);
        }

        DayCycleController.Instance.StartDay();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            ForceEndNight();

        if (Input.GetKeyDown(KeyCode.L))
        {
            InventoryManager.Instance.AddGold(100);
        }
    }

    private void HandleHordeCompleted()
    {
        RewardsSystem.Instance?.EvaluateAndGrantReward();
        DayCycleController.Instance.StartDay();
    }

    private void OnDestroy() 
    { 
        if (GameFlowController.Instance != null) GameFlowController.Instance.OnPhaseChanged -= HandlePhaseChanged; 
    }

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        bool isNight = newPhase == GamePhase.Night;
        foreach (var spawnpoint in spawnpoints)
        {
            if (spawnpoint != null) spawnpoint.SetNightMode(isNight);
        }

        if (isNight)
        {
            RewardsSystem.Instance?.StartNightEvaluation();
        }
    }

    public void ForceEndNight()
    {
        foreach (var altar in FindObjectsOfType<RitualAltar>())
            altar.ForceStopRitual();

        var spawner = FindObjectOfType<EnemiesSpawner>();
        if (spawner != null) spawner.EndNight();
        else GameFlowController.Instance.SetPhase(GamePhase.Day);
        DayCycleController.Instance.StartDay();
    }
}
