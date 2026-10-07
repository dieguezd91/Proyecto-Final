using UnityEngine;
using System;

public class PauseController : MonoBehaviour
{
    public bool IsPaused { get; private set; }

    public event Action<bool> OnPauseStateChanged;
    private float previousTimeScale = 1f;

    public void Pause()
    {
        if (IsPaused) return;
        previousTimeScale = UpgradeRuntime.GameplayBlocked && UpgradeRuntime.Current != null
            ? UpgradeRuntime.Current.OwnedTimeScale : Time.timeScale;
        IsPaused = true;
        Time.timeScale = 0f;
        OnPauseStateChanged?.Invoke(IsPaused);
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        if (!UpgradeRuntime.GameplayBlocked) Time.timeScale = previousTimeScale;
        OnPauseStateChanged?.Invoke(IsPaused);
    }

    public void Toggle()
    {
        if (IsPaused) Resume();
        else Pause();
    }
}
