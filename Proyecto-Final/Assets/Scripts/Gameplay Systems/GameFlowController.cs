using System;
using System.Collections.Generic;
using UnityEngine;

public enum GamePhase
{
    None = 0,
    GameOver = 1,
    Day = 2,
    Night = 3,
    MainMenu = 8,
    OnRitual = 10
}

public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance { get; private set; }

    [SerializeField] private GamePhase currentPhase = GamePhase.None;
    [SerializeField] private WorldTransitionAnimator worldAnimator;

    public GamePhase CurrentPhase => currentPhase;

    public event Action<GamePhase> OnPhaseChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetPhase(GamePhase newPhase)
    {
        if (currentPhase == newPhase)
            return;

        HandleWorldTransition(newPhase);

        currentPhase = newPhase;
        OnPhaseChanged?.Invoke(currentPhase);
    }

    private void HandleWorldTransition(GamePhase newPhase)
    {
        if (worldAnimator == null)
        {
            worldAnimator = FindObjectOfType<WorldTransitionAnimator>();
            if (worldAnimator == null) return;
        }

        HashSet<GamePhase> nonTransitionStates = new HashSet<GamePhase>
        {
            GamePhase.GameOver,
            GamePhase.OnRitual
        };

        if (nonTransitionStates.Contains(newPhase))
        {
            return;
        }

        switch (newPhase)
        {
            case GamePhase.Day:
                worldAnimator.TransitionToDay();
                break;

            case GamePhase.Night:
                worldAnimator.TransitionToNight();
                break;
        }
    }
}







