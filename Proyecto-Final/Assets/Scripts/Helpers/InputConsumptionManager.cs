using UnityEngine;

public static class InputConsumptionManager
{
    private static bool escapeConsumedThisFrame = false;
    private static int lastConsumedFrame = -1;

    public static bool IsEscapeConsumed
    {
        get
        {
            if (Time.frameCount != lastConsumedFrame)
            {
                escapeConsumedThisFrame = false;
            }
            return escapeConsumedThisFrame;
        }
    }

    public static void ConsumeEscape()
    {
        escapeConsumedThisFrame = true;
        lastConsumedFrame = Time.frameCount;
    }

    public static void Reset()
    {
        escapeConsumedThisFrame = false;
        lastConsumedFrame = -1;
    }
}