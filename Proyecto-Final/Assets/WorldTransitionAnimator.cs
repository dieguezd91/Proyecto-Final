using System;
using UnityEngine;

public class WorldTransitionAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator worldAnimator;
    [SerializeField] private string dayStateName = "BaseMap";
    [SerializeField] private string nightStateName = "HellMap";
    [SerializeField] private string transitionTrigger = "ChangeWorld";

    private bool isNightMode = false;
    public bool IsNightMode => isNightMode;

    public Action<bool> OnWorldModeChanged;

    void Start()
    {
        if (worldAnimator == null)
            worldAnimator = GetComponent<Animator>();

        SetWorldMode(false, false);
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

    public void ForceDayMode()
    {
        SetWorldMode(false, false);
    }

    public void ForceNightMode()
    {
        SetWorldMode(true, false);
    }

    private void TransitionToMode(bool toNight)
    {
        if (worldAnimator == null) return;

        isNightMode = toNight;

        if (!string.IsNullOrEmpty(transitionTrigger))
        {
            worldAnimator.SetTrigger(transitionTrigger);
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