using UnityEngine;
using UnityEngine.UI;

public class UICursor : MonoBehaviour
{
    [System.Serializable]
    public struct CursorData
    {
        public Sprite cursorSprite;
        public Vector2 hotSpot;
    }

    [Header("Cursor Settings")]
    [SerializeField] private CursorData defaultCursor;
    [SerializeField] private CursorData menuCursor;
    [SerializeField] private CursorData dayCursor;
    [SerializeField] private CursorData nightCursor;
    [SerializeField] private CursorData diggingCursor;
    [SerializeField] private CursorData plantingCursor;
    [SerializeField] private CursorData harvestingCursor;

    [Header("UI Components")]
    [SerializeField] private Image cursorImage;

    [SerializeField] private Grid grid;
    PlayerAbilitySystem playerAbilitySystem;

    private void Start()
    {
        Cursor.visible = false;
        SetCursorForGameState(GameState.Day);
        playerAbilitySystem = FindObjectOfType<PlayerAbilitySystem>();
        cursorImage.transform.SetAsLastSibling();
    }

    private void Update()
    {
        CursorData activeCursor = GetCurrentCursorData();
        GameState state = GameManager.Instance.currentGameState;

        bool useTileSnap = IsUsingTileSnap(state);
        bool inRange = IsTargetInRange(state);

        CursorData cursorToUse = useTileSnap && inRange ? activeCursor : defaultCursor;

        if (useTileSnap && inRange)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = grid.WorldToCell(worldPos);
            Vector3 snappedWorldPos = grid.GetCellCenterWorld(cellPos);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(snappedWorldPos);

            cursorImage.rectTransform.position = screenPos + (Vector3)cursorToUse.hotSpot;
        }
        else
        {
            cursorImage.rectTransform.position = (Vector3)Input.mousePosition + (Vector3)cursorToUse.hotSpot;
        }

        cursorImage.sprite = cursorToUse.cursorSprite;
        cursorImage.SetNativeSize();
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

        if (cursorImage != null)
        {
            cursorImage.sprite = cursorToUse.cursorSprite;
            cursorImage.SetNativeSize();
        }
    }

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
    private bool IsUsingTileSnap(GameState state)
    {
        return state == GameState.Digging || state == GameState.Planting || state == GameState.Harvesting;
    }

    private bool IsTargetInRange(GameState state)
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cell = grid.WorldToCell(mouseWorld);
        Vector3 cellWorld = grid.GetCellCenterWorld(cell);

        if (playerAbilitySystem == null) return false;

        float range = 0f;

        switch (state)
        {
            case GameState.Digging:
                range = playerAbilitySystem.digDistance;
                break;
            case GameState.Planting:
            case GameState.Harvesting:
                range = playerAbilitySystem.interactionDistance;
                break;
            default:
                return false;
        }

        return Vector2.Distance(playerAbilitySystem.transform.position, cellWorld) <= range;
    }
}
