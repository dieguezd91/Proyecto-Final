using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum PlayerAbility
{
    None,
    Planting,
    Harvesting,
    Digging,
    Removing
}

public class PlayerAbilitySystem : MonoBehaviour
{
    [Header("Dig System")]
    [SerializeField] public LayerMask diggableLayer;
    [SerializeField] public float digDistance = 2f;
    [SerializeField] private float digDuration = 1.5f;

    [Header("Harvest System")]
    [SerializeField] public float interactionDistance = 2f;

    [Header("Progress Visualization")]
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private Transform progressBarTarget;
    [SerializeField] private Vector3 progressBarOffset = new Vector3(0, 1.5f, 0);

    [Header("References")]
    [SerializeField] private SeedInventory plantInventory;
    [SerializeField] public TileBase tilledSoilTile;

    private PlayerAbility currentAbility = PlayerAbility.Digging;
    private PlayerController playerController;
    private bool isDigging = false;
    private Vector3 digPosition;

    private FloatingTextController floatingText;

    private bool isHarvesting = false;
    private ResourcePlant currentHarvestPlant = null;

    public delegate void AbilityChangedHandler(PlayerAbility newAbility);
    public event AbilityChangedHandler OnAbilityChanged;

    public PlayerAbility CurrentAbility => currentAbility;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        progressBar ??= FindObjectOfType<ProgressBar>();
        progressBarTarget ??= transform;
        plantInventory ??= FindObjectOfType<SeedInventory>();

        if (plantInventory != null)
        {
            plantInventory.onSlotSelected += OnSeedSlotSelected;
        }

        floatingText = GetComponentInChildren<FloatingTextController>();
    }

    private void OnDestroy()
    {
        if (plantInventory != null)
        {
            plantInventory.onSlotSelected -= OnSeedSlotSelected;
        }
    }

    private void Start()
    {
        SetAbility(PlayerAbility.Digging);
    }

    private void Update()
    {
        if ((isDigging || isHarvesting) && progressBar != null && progressBarTarget != null)
        {
            progressBar.transform.position = Camera.main.WorldToScreenPoint(
                progressBarTarget.position + progressBarOffset);
        }

        if (currentAbility == PlayerAbility.Digging && !isDigging)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cell = TilePlantingSystem.Instance.PlantingTilemap.WorldToCell(mouseWorld);
            Vector3 cellWorldPos = TilePlantingSystem.Instance.PlantingTilemap.GetCellCenterWorld(cell);
            float dist = Vector2.Distance(transform.position, cellWorldPos);
        }

        if (GameManager.Instance.currentGameState == GameState.Paused || PauseMenu.isGamePaused)
            return;
        if (!IsAbilityGameState())
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelCurrentAction();
            return;
        }

        switch (currentAbility)
        {
            case PlayerAbility.Planting:
                HandlePlanting();
                break;
            case PlayerAbility.Harvesting:
                HandleHarvesting();
                break;
            case PlayerAbility.Digging:
                HandleDigging();
                break;
            case PlayerAbility.Removing:
                HandleRemoving();
                break;
        }

        HandleMouseScroll();
    }

    private void HandleMouseScroll()
    {
        if (isDigging || isHarvesting)
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            PlayerAbility[] validAbilities = {
                PlayerAbility.Digging,
                PlayerAbility.Planting,
                PlayerAbility.Harvesting,
                PlayerAbility.Removing
            };

            int currentIndex = System.Array.IndexOf(validAbilities, currentAbility);
            if (currentIndex == -1) currentIndex = 0;

            int nextIndex = scroll > 0
                ? (currentIndex - 1 + validAbilities.Length) % validAbilities.Length
                : (currentIndex + 1) % validAbilities.Length;

            SetAbility(validAbilities[nextIndex]);
        }
    }

    private void OnSeedSlotSelected(int slotIndex)
    {
        SetAbility(PlayerAbility.Planting);
    }

    public void SetAbility(PlayerAbility ability)
    {
        CancelCurrentAction();

        currentAbility = ability;
        OnAbilityChanged?.Invoke(currentAbility);

        switch (currentAbility)
        {
            case PlayerAbility.Digging:
                GameManager.Instance.SetGameState(GameState.Digging);
                break;
            case PlayerAbility.Planting:
                GameManager.Instance.SetGameState(GameState.Planting);
                break;
            case PlayerAbility.Harvesting:
                GameManager.Instance.SetGameState(GameState.Harvesting);
                break;
            case PlayerAbility.Removing:
                GameManager.Instance.SetGameState(GameState.Removing);
                break;
            default:
                GameManager.Instance.SetGameState(GameState.Digging);
                break;
        }
    }

    private void HandlePlanting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = TilePlantingSystem.Instance.PlantingTilemap.WorldToCell(mouseWorld);
            GameObject selectedPlant = plantInventory.GetSelectedPlantPrefab();
            if (selectedPlant == null) return;

            if (!plantInventory.HasSeedsInSelectedSlot())
            {
                floatingText?.ShowWarning("No seeds left.");
                return;
            }

            Vector3 center = TilePlantingSystem.Instance.PlantingTilemap.GetCellCenterWorld(cellPos);

            if (Vector2.Distance(transform.position, center) > interactionDistance)
            {
                floatingText?.ShowWarning("Too far to plant.");
                return;
            }

            bool planted = TilePlantingSystem.Instance.TryPlant(cellPos, selectedPlant, out string reason);

            if (planted)
            {
                SoundManager.Instance.PlayOneShot("Plant");
                plantInventory.ConsumeSeedInSelectedSlot();
                GameManager.Instance.uiManager.UpdateSeedCountsUI();
                GameManager.Instance.uiManager.InitializeSeedSlotsUI();
            }
            else if (!string.IsNullOrEmpty(reason))
            {
                floatingText?.ShowWarning(reason);
            }
        }
    }

    private void HandleHarvesting()
    {
        if (isHarvesting)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = TilePlantingSystem.Instance.PlantingTilemap.WorldToCell(mouseWorld);

            Plant possiblePlant = TilePlantingSystem.Instance.GetPlantAt(cellPos);
            if (possiblePlant is ResourcePlant harvestablePlant)
            {
                if (!harvestablePlant.IsReadyToHarvest())
                {
                    floatingText?.ShowWarning("Plant not ready to harvest.");
                    return;
                }
                if (harvestablePlant.IsBeingHarvested())
                {
                    floatingText?.ShowWarning("This plant is already being harvested.");
                    return;
                }
                if (Vector2.Distance(transform.position, harvestablePlant.transform.position) > interactionDistance)
                {
                    floatingText?.ShowWarning("Too far to harvest.");
                    return;
                }

                StartHarvesting(harvestablePlant);
            }
            else
            {
                floatingText?.ShowWarning("No harvestable plant here.");
            }
        }
    }

    private void HandleRemoving()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = TilePlantingSystem.Instance.PlantingTilemap.WorldToCell(mouseWorld);

            Plant plant = TilePlantingSystem.Instance.GetPlantAt(cellPos);
            if (plant == null)
            {
                floatingText?.ShowWarning("No plant to remove.");
                return;
            }

            if (Vector2.Distance(transform.position, plant.transform.position) > interactionDistance)
            {
                floatingText?.ShowWarning("Too far to remove plant.");
                return;
            }
            SoundManager.Instance.PlayOneShot("Remove");

            TilePlantingSystem.Instance.UnregisterPlantAt(cellPos);
            Destroy(plant.gameObject);
            floatingText?.ShowWarning("Plant removed.");
        }
    }

    public void StartHarvesting(ResourcePlant plant)
    {
        if (currentAbility != PlayerAbility.Harvesting || plant == null || !plant.IsReadyToHarvest() || plant.IsBeingHarvested())
            return;

        currentHarvestPlant = plant;
        isHarvesting = true;
        playerController.SetMovementEnabled(false);
        currentHarvestPlant.GetComponent<SpriteRenderer>().color = currentHarvestPlant.clickColor;
        SoundManager.Instance.PlayOneShot("Harvest");
        progressBar?.SetImmediateProgress(0f);
        progressBar?.Show(true);
        progressBar?.gameObject.SetActive(true);

        plant.StartHarvest();
        StartCoroutine(MonitorHarvest());
    }

    private IEnumerator MonitorHarvest()
    {
        float startTime = Time.time;
        float harvestDuration = currentHarvestPlant.GetHarvestDuration();

        while (isHarvesting && currentHarvestPlant != null)
        {
            if (GameManager.Instance.currentGameState == GameState.Paused || PauseMenu.isGamePaused)
            {
                yield return null;
                continue;
            }

            float elapsed = Time.time - startTime;
            progressBar?.SetProgress(Mathf.Clamp01(elapsed / harvestDuration));

            if (Vector2.Distance(transform.position, currentHarvestPlant.transform.position) > interactionDistance)
            {
                CancelHarvest();
                yield break;
            }

            if (!currentHarvestPlant.IsBeingHarvested())
            {
                CompleteHarvest();
                yield break;
            }

            yield return null;
        }
    }

    public void CancelHarvest()
    {
        if (!isHarvesting || currentHarvestPlant == null)
            return;

        currentHarvestPlant.CancelHarvest();
        isHarvesting = false;
        playerController.SetMovementEnabled(true);
        progressBar?.Hide();
        currentHarvestPlant = null;
    }

    private void CompleteHarvest()
    {
        isHarvesting = false;
        playerController.SetMovementEnabled(true);
        progressBar?.Hide();
        currentHarvestPlant = null;
    }

    private void HandleDigging()
    {
        if (isDigging)
            return;

        Collider2D roofHit = Physics2D.OverlapPoint(transform.position, LayerMask.GetMask("House"));
        if (roofHit != null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            playerController.SetMovementEnabled(false);
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TryDig(mousePos);
        }
    }

    public bool TryDig(Vector2 position)
    {
        if (currentAbility != PlayerAbility.Digging || isDigging)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, diggableLayer);
        if (hit.collider == null)
            return false;
        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("House"))
            return false;

        Vector3Int cell = TilePlantingSystem.Instance.PlantingTilemap.WorldToCell(position);
        TileBase existingTile = TilePlantingSystem.Instance.PlantingTilemap.GetTile(cell);
        if (existingTile == tilledSoilTile)
        {
            Vector3 warnPos = transform.position + Vector3.up * 1.5f;
            floatingText?.ShowWarning("There is already tilled soil at this position.");
            return false;
        }

        float distance = Vector2.Distance(transform.position, position);
        if (distance > digDistance)
        {
            Vector3 warnPos = transform.position + Vector3.up * 1.5f;
            floatingText?.ShowWarning("The spot is too far away.");
            return false;
        }

        StartDigging(position);
        return true;
    }

    private void StartDigging(Vector2 position)
    {
        if (GameManager.Instance.currentGameState == GameState.Paused || PauseMenu.isGamePaused)
            return;
        SoundManager.Instance.PlayOneShot("Dig");
        isDigging = true;
        digPosition = new Vector3(position.x, position.y, 0);
        playerController.SetMovementEnabled(false);
        progressBar?.SetImmediateProgress(0f);
        progressBar?.Show(false);

        StartCoroutine(DiggingProcess());
    }

    private IEnumerator DiggingProcess()
    {
        float timer = 0;

        while (timer < digDuration)
        {
            if (GameManager.Instance.currentGameState == GameState.Paused || PauseMenu.isGamePaused)
            {
                yield return null;
                continue;
            }

            timer += Time.deltaTime;
            progressBar?.SetProgress(timer / digDuration);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelDigging();
                yield break;
            }

            yield return null;
        }

        CompleteDigging();
    }

    private void CompleteDigging()
    {
        Vector3Int cell = TilePlantingSystem.Instance.PlantingTilemap.WorldToCell(digPosition);
        TilePlantingSystem.Instance.PlantingTilemap.SetTile(cell, tilledSoilTile);
        isDigging = false;
        playerController.SetMovementEnabled(true);
        progressBar?.Hide();
    }

    private void CancelDigging()
    {
        isDigging = false;
        playerController.SetMovementEnabled(true);
        progressBar?.Hide();
    }

    private void CancelCurrentAction()
    {
        if (isHarvesting)
            CancelHarvest();
        if (isDigging)
            CancelDigging();
    }

    public bool IsDigging() => isDigging;
    public bool IsHarvesting() => isHarvesting;

    private bool IsAbilityGameState()
    {
        var state = GameManager.Instance.currentGameState;
        return state == GameState.Day ||
               state == GameState.Digging ||
               state == GameState.Planting ||
               state == GameState.Harvesting ||
               state == GameState.Removing;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, digDistance);
    }
}
