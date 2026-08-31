using UnityEngine;
using System;

public class PauseController : MonoBehaviour
{
    public bool IsPaused { get; private set; }

    public event Action<bool> OnPauseStateChanged;

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;
        Time.timeScale = 0f;
        OnPauseStateChanged?.Invoke(IsPaused);
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = 1f;
        OnPauseStateChanged?.Invoke(IsPaused);
    }

    public void Toggle()
    {
        if (IsPaused) Resume();
        else Pause();
    }
}
