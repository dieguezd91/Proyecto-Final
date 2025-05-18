using UnityEngine;

public class CursorController : MonoBehaviour
{
    [System.Serializable]
    public struct CursorData
    {
        public Texture2D cursorTexture;
        public Vector2 hotSpot;
        public CursorMode cursorMode;
    }

    [Header("Cursor Settings")]
    [SerializeField] private CursorData defaultCursor;
    [SerializeField] private CursorData menuCursor;
    [SerializeField] private CursorData dayCursor;
    [SerializeField] private CursorData nightCursor;
    [SerializeField] private CursorData diggingCursor;
    [SerializeField] private CursorData plantingCursor;
    [SerializeField] private CursorData harvestingCursor;

    private void Start()
    {
        SetCursorForGameState(GameState.Day);
    }

    public void SetCursorForGameState(GameState state)
    {
        CursorData cursorToUse = defaultCursor;

        switch (state)
        {
            case GameState.MainMenu:
            case GameState.Paused:
            case GameState.OnInventory:
            case GameState.OnCrafting:
                cursorToUse = menuCursor;
                break;

            case GameState.Day:
                cursorToUse = dayCursor;
                break;

            case GameState.Night:
                cursorToUse = nightCursor;
                break;

            case GameState.Digging:
                cursorToUse = diggingCursor;
                break;

            case GameState.Planting:
                cursorToUse = plantingCursor;
                break;
                
            case GameState.Harvesting:
                cursorToUse = harvestingCursor;
                break;
        }

        Cursor.SetCursor(cursorToUse.cursorTexture, cursorToUse.hotSpot, cursorToUse.cursorMode);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || Camera.main == null) return;

        CursorData activeCursor = GetCurrentCursorData();
        Vector3 mouseScreenPos = Input.mousePosition;

        Vector3 adjustedScreenPos = mouseScreenPos + (Vector3)activeCursor.hotSpot;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(adjustedScreenPos.x, adjustedScreenPos.y, Camera.main.nearClipPlane + 1f));

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(worldPos, Vector3.one * 0.2f);
    }
#endif

    private CursorData GetCurrentCursorData()
    {
        switch (GameManager.Instance.currentGameState)
        {
            case GameState.MainMenu:
            case GameState.Paused:
            case GameState.OnInventory:
            case GameState.OnCrafting:
                return menuCursor;

            case GameState.Day:
                return dayCursor;

            case GameState.Night:
                return nightCursor;

            case GameState.Digging:
                return diggingCursor;

            case GameState.Planting:
                return plantingCursor;

            case GameState.Harvesting:
                return harvestingCursor;

            default:
                return defaultCursor;
        }
    }
}