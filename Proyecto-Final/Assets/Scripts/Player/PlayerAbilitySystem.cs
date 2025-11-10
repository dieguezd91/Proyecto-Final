using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
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
    [Header("DIG ABILITY")]
    [SerializeField] public LayerMask diggableLayer;
    [SerializeField] public float digDistance = 2f;
    [SerializeField] private float digDuration = 1.5f;
    [SerializeField] private float digManaCost;

    [Header("HARVEST ABILITY")]
    [SerializeField] public float interactionDistance = 2f;
    [SerializeField] private float harvestManaCost;

    [Header("PLANTING ABILITY")]
    [SerializeField] private float plantManaCost;

    [Header("REMOVING ABILITY")]
    [SerializeField] private float removeManaCost;

    [Header("PROGRESS FEEDBACK")]
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private Transform progressBarTarget;
    [SerializeField] private Vector3 progressBarOffset = new Vector3(0, 1.5f, 0);

    [Header("REFERENCES")]
    [SerializeField] private SeedInventory seedInventory;
    [SerializeField] public TileBase tilledSoilTile;
    private ManaSystem manaSystem;

    private PlayerAbility currentAbility = PlayerAbility.Digging;
    private PlayerController playerController;
    private bool isDigging = false;
    private Vector3 digPosition;

    private WarningBubble warningBubble;
    private FloatingTextController floatingTextController;

    private bool isHarvesting = false;
    private ResourcePlant currentHarvestPlant = null;

    public delegate void AbilityChangedHandler(PlayerAbility newAbility);
    public event AbilityChangedHandler OnAbilityChanged;

    [SerializeField] private GameObject digAnimationPrefab;

    private bool initialized = false;

    public PlayerAbility CurrentAbility => currentAbility;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        manaSystem = GetComponent<ManaSystem>();
        progressBar ??= FindObjectOfType<ProgressBar>();
        progressBarTarget ??= transform;
        seedInventory ??= FindObjectOfType<SeedInventory>();

        if (seedInventory != null)
        {
            seedInventory.onSlotSelected += OnSeedSlotSelected;
        }

        warningBubble = GetComponentInChildren<WarningBubble>();
        floatingTextController = GetComponentInChildren<FloatingTextController>();
    }

    private void OnDestroy()
    {
        if (seedInventory != null)
        {
            seedInventory.onSlotSelected -= OnSeedSlotSelected;
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

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (currentAbility == PlayerAbility.Digging && !isDigging)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cell = TilePlantingSystem.Instance.PlantingTilemap.WorldToCell(mouseWorld);
            Vector3 cellWorldPos = TilePlantingSystem.Instance.PlantingTilemap.GetCellCenterWorld(cell);
            float dist = Vector2.Distance(transform.position, cellWorldPos);
        }

        if (!isDigging && !isHarvesting)
        {
            HandleMouseScroll();
        }

        if (LevelManager.Instance.currentGameState == GameState.Paused || GameManager.Instance.IsGamePaused())
            return;

        if (!IsAbilityGameState())
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelCurrentAction();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            CycleAbility(-1);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            CycleAbility(1);
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
    }

    private void CycleAbility(int direction)
    {
        if (isDigging || isHarvesting)
            return;

        if (!IsAbilityGameState())
            return;

        PlayerAbility[] validAbilities = {
            PlayerAbility.Digging,
            PlayerAbility.Planting,
            PlayerAbility.Harvesting,
            PlayerAbility.Removing
        };

        int currentIndex = System.Array.IndexOf(validAbilities, currentAbility);
        if (currentIndex == -1) currentIndex = 0;

        int nextIndex = (currentIndex + direction + validAbilities.Length) % validAbilities.Length;

        SetAbility(validAbilities[nextIndex]);
    }

    private void HandleMouseScroll()
    {
        if (isDigging || isHarvesting)
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            int direction = scroll > 0 ? -1 : 1;
            CycleAbility(direction);
        }
    }

    private void OnSeedSlotSelected(int slotIndex)
    {
        if (!initialized)
        {
            initialized = true;
            return;
        }

        if (LevelManager.Instance.currentGameState == GameState.Night)
            return;

        SetAbility(PlayerAbility.Planting);
    }

    public void SetAbility(PlayerAbility ability)
    {
        CancelCurrentAction();

        if (currentAbility == ability) return;

        currentAbility = ability;
        OnAbilityChanged?.Invoke(currentAbility);

        switch (currentAbility)
        {
            case PlayerAbility.Digging:
                LevelManager.Instance.SetGameState(GameState.Digging);
                break;
            case PlayerAbility.Planting:
                LevelManager.Instance.SetGameState(GameState.Planting);
                break;
            case PlayerAbility.Harvesting:
                LevelManager.Instance.SetGameState(GameState.Harvesting);
                break;
            case PlayerAbility.Removing:
                LevelManager.Instance.SetGameState(GameState.Removing);
                break;
            default:
                LevelManager.Instance.SetGameState(GameState.Digging);
                break;
        }
    }

    private void HandlePlanting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = TilePlantingSystem.Instance.PlantingTilemap.WorldToCell(mouseWorld);
            GameObject selectedPlant = seedInventory.GetSelectedPlantPrefab();
            if (selectedPlant == null) return;

            if (!seedInventory.HasSeedsInSelectedSlot())
            {
                warningBubble?.ShowMessage("No seeds left.");
                return;
            }

            Vector3 center = TilePlantingSystem.Instance.PlantingTilemap.GetCellCenterWorld(cellPos);

            if (Vector2.Distance(transform.position, center) > interactionDistance)
            {
                warningBubble?.ShowMessage("Too far to plant.");
                return;
            }

            if (manaSystem != null && !manaSystem.UseMana(plantManaCost))
            {
                warningBubble?.ShowMessage("Not enough mana to plant!");

                if (floatingTextController != null)
                {
                    warningBubble.ShowMessage("Insufficient Mana");
                }

                SoundManager.Instance?.PlayOneShot("Error");
                return;
            }

            bool planted = TilePlantingSystem.Instance.TryPlant(cellPos, selectedPlant, out string reason);

            if (planted)
            {
                seedInventory.ConsumeSeedInSelectedSlot();
                SoundManager.Instance.Play("Plant");
            }
            else
            {
                warningBubble?.ShowMessage(reason);
            }
        }
    }

    private void HandleHarvesting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider == null)
                return;

            ResourcePlant plant = hit.collider.GetComponent<ResourcePlant>();
            if (plant == null || plant.IsBeingHarvested())
                return;

            if (Vector2.Distance(transform.position, hit.collider.transform.position) > interactionDistance)
            {
                warningBubble?.ShowMessage("Too far to harvest.");
                return;
            }

            if (!plant.IsReadyToHarvest())
            {
                warningBubble?.ShowMessage("Not ready to harvest yet!");
                return;
            }

            if (manaSystem != null && !manaSystem.UseMana(harvestManaCost))
            {
                warningBubble?.ShowMessage("Not enough mana to harvest!");

                if (floatingTextController != null)
                {
                    warningBubble.ShowMessage("Insufficient Mana");
                }

                SoundManager.Instance?.PlayOneShot("Error");
                return;
            }

            StartHarvesting(plant);
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
                warningBubble?.ShowMessage("No plant to remove.");
                return;
            }

            if (Vector2.Distance(transform.position, plant.transform.position) > interactionDistance)
            {
                warningBubble?.ShowMessage("Too far to remove plant.");
                return;
            }

            if (manaSystem != null && !manaSystem.UseMana(removeManaCost))
            {
                warningBubble?.ShowMessage("Not enough mana to remove!");

                if (floatingTextController != null)
                {
                    warningBubble.ShowMessage("Insufficient Mana");
                }

                SoundManager.Instance?.PlayOneShot("Error");
                return;
            }

            SoundManager.Instance.Play("Remove");
            TilePlantingSystem.Instance.UnregisterPlantAt(cellPos);
            Destroy(plant.gameObject);
            warningBubble?.ShowMessage("Plant removed.");
        }
    }

    public void StartHarvesting(ResourcePlant plant)
    {
        if (currentAbility != PlayerAbility.Harvesting || plant == null || !plant.IsReadyToHarvest() || plant.IsBeingHarvested())
            return;

        currentHarvestPlant = plant;
        isHarvesting = true;
        currentHarvestPlant.GetComponent<SpriteRenderer>().color = currentHarvestPlant.clickColor;
        SoundManager.Instance.Play("Harvest");
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
            if (LevelManager.Instance.currentGameState == GameState.Paused || GameManager.Instance.IsGamePaused())
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
        progressBar?.Hide();

        currentHarvestPlant = null;
    }

    private void CompleteHarvest()
    {
        isHarvesting = false;
        progressBar?.Hide();
        var reward = currentHarvestPlant.GetHarvestReward();
        if (reward != null)
        {
            floatingTextController?.ShowPickup(reward.materialName, reward.amount, reward.icon);
        }
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
            warningBubble?.ShowMessage("There is already tilled soil at this position.");
            return false;
        }

        float distance = Vector2.Distance(transform.position, position);
        if (distance > digDistance)
        {
            warningBubble?.ShowMessage("The spot is too far away.");
            return false;
        }

        if (manaSystem != null && !manaSystem.UseMana(digManaCost))
        {
            warningBubble?.ShowMessage("Not enough mana to dig!");

            if (floatingTextController != null)
            {
                warningBubble.ShowMessage("Insufficient Mana");
            }

            SoundManager.Instance?.PlayOneShot("Error");
            return false;
        }

        StartDigging(position);
        return true;
    }

    private void StartDigging(Vector2 position)
    {
        if (LevelManager.Instance.currentGameState == GameState.Paused || GameManager.Instance.IsGamePaused())
            return;

        isDigging = true;
        digPosition = new Vector3(position.x, position.y, 0);
        progressBar?.SetImmediateProgress(0f);
        progressBar?.Show(false);

        if (digAnimationPrefab != null)
        {
            Vector3Int cell = TilePlantingSystem.Instance.PlantingTilemap.WorldToCell(digPosition);
            Vector3 spawnPos = TilePlantingSystem.Instance.PlantingTilemap.GetCellCenterWorld(cell);
            Instantiate(digAnimationPrefab, spawnPos, Quaternion.identity);
            SoundManager.Instance.Play("Dig");
        }

        StartCoroutine(DiggingProcess());
    }

    private IEnumerator DiggingProcess()
    {
        float timer = 0;

        while (timer < digDuration)
        {
            if (LevelManager.Instance.currentGameState == GameState.Paused || GameManager.Instance.IsGamePaused())
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
        progressBar?.Hide();
    }

    private void CancelDigging()
    {
        isDigging = false;
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
        var state = LevelManager.Instance.currentGameState;
        return state == GameState.Day ||
               state == GameState.Digging ||
               state == GameState.Planting ||
               state == GameState.Harvesting ||
               state == GameState.Removing;
    }

    public bool IsBusy()
    {
        return isDigging || isHarvesting;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, digDistance);
    }
}