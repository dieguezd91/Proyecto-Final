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
    private PauseController pauseController;

    private void Start()
    {
        if (grid == null)
        {
            Destroy(gameObject);
            return;
        }

        Cursor.visible = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        
        playerAbilitySystem = FindObjectOfType<PlayerAbilitySystem>();
        pauseController = FindObjectOfType<PauseController>();
        if (cursorImage != null)
        {
            cursorImage.transform.SetAsLastSibling();
        }
    }

    private void Update()
    {
        if (LevelManager.Instance == null)
            return;

        GamePhase phase = GameFlowController.Instance.CurrentPhase;

        if (playerAbilitySystem != null && playerAbilitySystem.IsBusy())
        {
            if (cursorImage != null && cursorImage.enabled)
                cursorImage.enabled = false;
            return;
        }

        bool isPaused = pauseController != null && pauseController.IsPaused;
        bool hasModal = UIManager.Instance?.Flow != null && UIManager.Instance.Flow.HasOpenModal;
        bool isGameplay = IsGameplayPhase(phase) && !hasModal && !isPaused;

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

        PlayerAbility ability = playerAbilitySystem != null ? playerAbilitySystem.CurrentAbility : PlayerAbility.None;
        bool useTileSnap = false;
        bool inRange = false;
        CursorData cursorToUse = defaultCursor;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = grid.WorldToCell(mouseWorld);
        var plant = TilePlantingSystem.Instance != null ? TilePlantingSystem.Instance.GetPlantAt(cellPos) : null;

        if (phase == GamePhase.Night)
        {
            cursorToUse = nightCursor;
        }
        else
        {
            useTileSnap = IsUsingTileSnap(ability);
            inRange = IsTargetInRange(ability);

            switch (ability)
            {
                case PlayerAbility.Digging:
                    if (plant != null)
                    {
                        cursorToUse = dayCursor;
                    }
                    else if (playerAbilitySystem != null)
                    {
                        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero, Mathf.Infinity, playerAbilitySystem.diggableLayer);
                        cursorToUse = hit.collider != null ? diggingCursor : dayCursor;
                    }
                    break;

                case PlayerAbility.Planting:
                    if (TilePlantingSystem.Instance != null && playerAbilitySystem != null)
                    {
                        var tile = TilePlantingSystem.Instance.PlantingTilemap.GetTile(cellPos);
                        cursorToUse = (plant == null && tile == playerAbilitySystem.tilledSoilTile) ? plantingCursor : dayCursor;
                    }
                    break;

                case PlayerAbility.Harvesting:
                    var harvestPlant = plant as ResourcePlant;
                    cursorToUse = (harvestPlant != null && harvestPlant.IsReadyToHarvest()) ? harvestingCursor : dayCursor;
                    break;

                case PlayerAbility.Removing:
                    cursorToUse = plant != null ? removingCursor : dayCursor;
                    break;

                default:
                    cursorToUse = dayCursor;
                    break;
            }

            if (!inRange && useTileSnap)
            {
                cursorToUse = dayCursor;
            }
        }

        bool shouldSnap = useTileSnap && inRange && cursorToUse.cursorSprite != dayCursor.cursorSprite;

        if (shouldSnap && cursorImage != null)
        {
            Vector3 snappedWorldPos = grid.GetCellCenterWorld(cellPos);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(snappedWorldPos);
            cursorImage.rectTransform.position = screenPos + (Vector3)cursorToUse.hotSpot;
        }
        else if (cursorImage != null)
        {
            cursorImage.rectTransform.position = (Vector3)Input.mousePosition + (Vector3)cursorToUse.hotSpot;
        }

        if (cursorImage != null)
        {
            cursorImage.sprite = cursorToUse.cursorSprite;
            cursorImage.SetNativeSize();
        }
    }

    private bool IsUsingTileSnap(PlayerAbility ability)
    {
        return ability != PlayerAbility.None;
    }

    private bool IsTargetInRange(PlayerAbility ability)
    {
        if (playerAbilitySystem == null) return false;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cell = grid.WorldToCell(mouseWorld);
        Vector3 cellWorld = grid.GetCellCenterWorld(cell);

        float range = 0f;

        switch (ability)
        {
            case PlayerAbility.Digging:
                range = playerAbilitySystem.digDistance;
                break;
            case PlayerAbility.Planting:
            case PlayerAbility.Harvesting:
            case PlayerAbility.Removing:
                range = playerAbilitySystem.interactionDistance;
                break;
            default:
                return false;
        }

        return Vector2.Distance(playerAbilitySystem.transform.position, cellWorld) <= range;
    }

    private bool IsGameplayPhase(GamePhase phase)
    {
        return phase != GamePhase.MainMenu && phase != GamePhase.GameOver;
    }
}

