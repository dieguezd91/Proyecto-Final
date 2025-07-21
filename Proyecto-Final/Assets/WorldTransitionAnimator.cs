using System;
using UnityEngine;

public class WorldTransitionAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator worldAnimator;
    [SerializeField] private string dayStateName = "BaseMap";
    [SerializeField] private string nightStateName = "HellMap";
    [SerializeField] private string transitionTrigger = "ChangeWorld";

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private bool isNightMode = false;
    public bool IsNightMode => isNightMode;
    private GameState lastGameState = GameState.None;

    public Action<bool> OnWorldModeChanged;

    void Start()
    {
        if (worldAnimator == null)
        {
            worldAnimator = GetComponent<Animator>();
        }

        if (worldAnimator == null)
        {
            return;
        }

        SetWorldMode(false, false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            lastGameState = GameManager.Instance.currentGameState;
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
    }

    private void OnGameStateChanged(GameState newState)
    {
        bool shouldBeNight = newState == GameState.Night;
        bool wasNight = lastGameState == GameState.Night;

        if (shouldBeNight != wasNight)
        {
            TransitionToMode(shouldBeNight);
        }

        lastGameState = newState;
    }

    public void TransitionToDay()
    {
        if (!isNightMode) return;
        TransitionToMode(false);
    }

    public void TransitionToNight()
    {
        if (isNightMode) return;
        TransitionToMode(true);
    }

    private void TransitionToMode(bool toNight)
    {
        if (worldAnimator == null)
        {
            return;
        }

        isNightMode = toNight;

        if (!string.IsNullOrEmpty(transitionTrigger))
        {
            bool hasParameter = false;
            foreach (AnimatorControllerParameter param in worldAnimator.parameters)
            {
                if (param.name == transitionTrigger && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasParameter = true;
                    break;
                }
            }

            if (hasParameter)
            {
                worldAnimator.SetTrigger(transitionTrigger);
            }
            else
            {
                string targetState = toNight ? nightStateName : dayStateName;
                worldAnimator.Play(targetState);
            }
        }
        else
        {
            string targetState = toNight ? nightStateName : dayStateName;
            worldAnimator.Play(targetState);
        }

        OnWorldModeChanged?.Invoke(isNightMode);
    }

    private void SetWorldMode(bool nightMode, bool useAnimation)
    {
        isNightMode = nightMode;

        if (worldAnimator != null)
        {
            string targetState = nightMode ? nightStateName : dayStateName;

            if (useAnimation)
            {
                worldAnimator.Play(targetState);
            }
            else
            {
                worldAnimator.Play(targetState, 0, 1f);
            }
        }
    }
}