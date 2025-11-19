using System;

public static class GameplayEvents
{
    public static event Action OnPlayerExitedHouseDuringNight;

    public static void InvokePlayerExitedHouseDuringNight()
        => OnPlayerExitedHouseDuringNight?.Invoke();
}