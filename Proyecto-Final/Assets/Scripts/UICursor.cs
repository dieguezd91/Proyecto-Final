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
    [SerializeField] private CursorData removingCursor;

    [Header("UI Components")]
    [SerializeField] private Image cursorImage;

    [SerializeField] private Grid grid;
    private PlayerAbilitySystem playerAbilitySystem;

    private void Start()
    {
        if (grid == null)
        {
            Destroy(gameObject);
            return;
        }

        Cursor.visible = false;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        GameState state = LevelManager.Instance != null ? LevelManager.Instance.currentGameState : GameState.MainMenu;

        SetCursorForGameState(state);
        playerAbilitySystem = FindObjectOfType<PlayerAbilitySystem>();
        cursorImage.transform.SetAsLastSibling();
    }

    private void Update()
    {
        if (LevelManager.Instance == null)
            return;

        GameState state = LevelManager.Instance.currentGameState;

        if (playerAbilitySystem != null && playerAbilitySystem.IsBusy())
        {
            if (cursorImage != null && cursorImage.enabled)
                cursorImage.enabled = false;
            return;
        }

        bool isGameplay = IsGameplayState(state);

        if (isGameplay)
        {
            if (Cursor.visible)
                Cursor.visible = false;

            if (cursorImage != null && !cursorImage.enabled)
                cursorImage.enabled = true;
        }
        else
        {
            if (!Cursor.visible)
                Cursor.visible = true;

            if (cursorImage != null && cursorImage.enabled)
                cursorImage.enabled = false;
        }

        if (!isGameplay || grid == null)
            return;

        bool useTileSnap = IsUsingTileSnap(state);
        bool inRange = IsTargetInRange(state);

        CursorData cursorToUse = defaultCursor;
        CursorData activeCursor = GetCurrentCursorData();

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = grid.WorldToCell(mouseWorld);
        var plant = TilePlantingSystem.Instance.GetPlantAt(cellPos);

        switch (state)
        {
            case GameState.Digging:
                if (plant != null)
                {
                    cursorToUse = dayCursor;
                }
                else
                {
                    RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero, Mathf.Infinity, playerAbilitySystem.diggableLayer);
                    cursorToUse = hit.collider != null ? diggingCursor : dayCursor;
                }
                break;

            case GameState.Planting:
                var tile = TilePlantingSystem.Instance.PlantingTilemap.GetTile(cellPos);
                cursorToUse = (plant == null && tile == playerAbilitySystem.tilledSoilTile) ? plantingCursor : dayCursor;
                break;

            case GameState.Harvesting:
                var harvestPlant = plant as ResourcePlant;
                cursorToUse = (harvestPlant != null && harvestPlant.IsReadyToHarvest()) ? harvestingCursor : dayCursor;
                break;

            case GameState.Removing:
                cursorToUse = plant != null ? removingCursor : dayCursor;
                break;

            default:
                cursorToUse = defaultCursor;
                break;
        }

        if (!inRange && IsUsingTileSnap(state))
        {
            cursorToUse = dayCursor;
        }


        bool shouldSnap = useTileSnap && inRange && cursorToUse.cursorSprite != dayCursor.cursorSprite;

        if (shouldSnap)
        {
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
        CursorData cursorToUse = GetCursorForState(state);

        if (cursorImage != null)
        {
            cursorImage.sprite = cursorToUse.cursorSprite;
            cursorImage.SetNativeSize();
        }
    }

    private CursorData GetCursorForState(GameState state)
    {
        switch (state)
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

            case GameState.Removing:
                return removingCursor;

            default:
                return defaultCursor;
        }
    }

    private CursorData GetCurrentCursorData()
    {
        return GetCursorForState(LevelManager.Instance.currentGameState);
    }

    private bool IsUsingTileSnap(GameState state)
    {
        return state == GameState.Digging || state == GameState.Planting || state == GameState.Harvesting || state == GameState.Removing;
    }

    private bool IsTargetInRange(GameState state)
    {
        if (playerAbilitySystem == null) return false;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cell = grid.WorldToCell(mouseWorld);
        Vector3 cellWorld = grid.GetCellCenterWorld(cell);

        float range = 0f;

        switch (state)
        {
            case GameState.Digging:
                range = playerAbilitySystem.digDistance;
                break;
            case GameState.Planting:
            case GameState.Harvesting:
            case GameState.Removing:
                range = playerAbilitySystem.interactionDistance;
                break;
            default:
                return false;
        }

        return Vector2.Distance(playerAbilitySystem.transform.position, cellWorld) <= range;
    }

    private bool IsGameplayState(GameState state)
    {
        return state == GameState.Digging || state == GameState.Planting ||
               state == GameState.Harvesting || state == GameState.Removing ||
               state == GameState.Day || state == GameState.Night;
    }
}
