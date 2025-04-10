using System.Collections;
using UnityEngine;

public enum PlayerAbility
{
    None,
    Planting,
    Harvesting,
    Digging
}

public class PlayerAbilitySystem : MonoBehaviour
{
    [Header("Dig System")]
    [SerializeField] private GameObject digSpotsContainer;
    [SerializeField] private GameObject plantingSpotPrefab;
    [SerializeField] private LayerMask diggableLayer;
    [SerializeField] private float digDistance = 2f;
    [SerializeField] private float digDuration = 1.5f;

    [Header("Plant System")]
    [SerializeField] public LayerMask plantingLayer; // Capa de parcelas

    [Header("Harvest System")]
    [SerializeField] public float interactionDistance = 2f; // Distancia maxima para interactuar con plantas

    [Header("Progress Visualization")]
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private Transform progressBarTarget;
    [SerializeField] private Vector3 progressBarOffset = new Vector3(0, 1.5f, 0);

    [Header("References")]
    [SerializeField] private PlantInventory plantInventory;

    private PlayerAbility currentAbility = PlayerAbility.Digging;
    private PlayerController playerController;
    private bool isDigging = false;
    private Vector3 digPosition;

    private bool isHarvesting = false;
    private ResourcePlant currentHarvestPlant = null;

    public delegate void AbilityChangedHandler(PlayerAbility newAbility);
    public event AbilityChangedHandler OnAbilityChanged;

    public PlayerAbility CurrentAbility => currentAbility;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();

        if (progressBar == null)
        {
            progressBar = FindObjectOfType<ProgressBar>();
        }

        if (progressBarTarget == null)
        {
            progressBarTarget = transform;
        }

        if (plantInventory == null)
        {
            plantInventory = FindObjectOfType<PlantInventory>();
        }
    }


    private void Start()
    {
        currentAbility = PlayerAbility.Digging;
        OnAbilityChanged?.Invoke(currentAbility);
    }

    private void Update()
    {
        if ((isDigging || isHarvesting) && progressBar != null && progressBarTarget != null)
        {
            progressBar.transform.position = Camera.main.WorldToScreenPoint(
                progressBarTarget.position + progressBarOffset);
        }

        if (GameManager.Instance.currentGameState == GameState.Paused || PauseMenu.isGamePaused)
        {
            return;
        }

        if (GameManager.Instance.currentGameState != GameState.Day)
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
        }
    }

    public void SetAbility(PlayerAbility ability)
    {
        if (currentAbility != ability)
        {
            CancelCurrentAction();

            currentAbility = ability;

            OnAbilityChanged?.Invoke(currentAbility);
        }
    }

    public bool TryPlant(PlantingSpot spot)
    {
        if (currentAbility != PlayerAbility.Planting)
        {
            Debug.LogWarning("No se puede plantar: la habilidad actual no es plantar");
            return false;
        }

        if (plantInventory == null)
            return false;

        GameObject selectedPlantPrefab = plantInventory.GetSelectedPlantPrefab();
        if (selectedPlantPrefab == null)
            return false;

        float distance = Vector2.Distance(transform.position, spot.transform.position);
        if (distance <= interactionDistance)
        {
            Debug.Log($"Plantando {plantInventory.GetSelectedPlantName()}");
            spot.Plant(selectedPlantPrefab);
            return true;
        }

        return false;
    }

    private void HandlePlanting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Intentando plantar");
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, plantingLayer);

            if (hit.collider != null)
            {
                PlantingSpot spot = hit.collider.GetComponent<PlantingSpot>();
                if (spot != null)
                {
                    TryPlant(spot);
                }
            }
        }
    }

    private void HandleHarvesting()
    {
        if (isHarvesting)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Intentando cosechar");
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] colliders = Physics2D.OverlapCircleAll(mousePos, 0.5f);

            bool plantFound = false;

            foreach (Collider2D collider in colliders)
            {
                PlantingSpot spot = collider.GetComponent<PlantingSpot>();
                if (spot != null)
                {
                    ResourcePlant plant = spot.GetPlantComponent<ResourcePlant>();
                    if (plant != null)
                    {
                        if (plant.IsReadyToHarvest() && !plant.IsBeingHarvested())
                        {
                            float distance = Vector2.Distance(transform.position, spot.transform.position);
                            if (distance <= interactionDistance)
                            {
                                plantFound = true;
                                StartHarvesting(plant);
                                return;
                            }
                        }
                    }
                }
            }

            if (!plantFound)
            {
                Debug.Log("No se encontro ninguna planta cosechable");
            }
        }
    }

    public void StartHarvesting(ResourcePlant plant)
    {
        if (currentAbility != PlayerAbility.Harvesting)
        {
            Debug.LogWarning("No se puede cosechar: la habilidad actual no es cosechar");
            return;
        }

        if (GameManager.Instance.currentGameState == GameState.Paused || PauseMenu.isGamePaused)
        {
            return;
        }

        if (plant == null || !plant.IsReadyToHarvest() || plant.IsBeingHarvested())
        {
            return;
        }

        currentHarvestPlant = plant;
        isHarvesting = true;
        playerController.SetMovementEnabled(false);

        if (progressBar != null)
        {
            Debug.Log("Mostrando barra de progreso para cosecha");
            progressBar.SetImmediateProgress(0f);
            progressBar.Show(true);

            progressBar.gameObject.SetActive(true);
        }

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

            if (progressBar != null)
            {
                float elapsed = Time.time - startTime;
                float progress = Mathf.Clamp01(elapsed / harvestDuration);
                progressBar.SetProgress(progress);

                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"Progreso de cosecha: {progress:F2}");
                }
            }

            if (Vector2.Distance(transform.position, currentHarvestPlant.transform.position) > interactionDistance)
            {
                Debug.Log("Cosecha cancelada: jugador fuera de rango");
                CancelHarvest();
                yield break;
            }

            if (!currentHarvestPlant.IsBeingHarvested())
            {
                Debug.Log("Cosecha completada naturalmente");
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

        if (progressBar != null)
        {
            progressBar.Hide();
        }

        Debug.Log("Cosecha cancelada");
        currentHarvestPlant = null;
    }

    public bool IsHarvesting()
    {
        return isHarvesting;
    }

    void CompleteHarvest()
    {
        isHarvesting = false;
        playerController.SetMovementEnabled(true);

        if (progressBar != null)
        {
            progressBar.Hide();
        }

        Debug.Log("Cosecha completada");
        currentHarvestPlant = null;
    }

    public bool TryDig(Vector2 position)
    {
        if (currentAbility != PlayerAbility.Digging)
        {
            Debug.LogWarning("No se puede cavar: la habilidad actual no es cavar");
            return false;
        }

        if (isDigging)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, diggableLayer);
        if (hit.collider == null)
            return false;

        Collider2D[] spots = Physics2D.OverlapCircleAll(position, 0.5f, plantingLayer);
        if (spots.Length > 0)
        {
            Debug.Log("Este lugar ya esta cavado");
            return false;
        }

        float distance = Vector2.Distance(transform.position, position);
        if (distance <= digDistance)
        {
            Debug.Log("Iniciando cavado");
            StartDigging(position);
            return true;
        }
        else
        {
            Debug.Log("El lugar está demasiado lejos");
            return false;
        }
    }

    private void HandleDigging()
    {
        if (isDigging)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Intentando cavar...");
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TryDig(mousePos);
        }
    }

    private void StartDigging(Vector2 position)
    {
        if (GameManager.Instance.currentGameState == GameState.Paused || PauseMenu.isGamePaused)
        {
            return;
        }

        isDigging = true;
        digPosition = new Vector3(position.x, position.y, 0);
        playerController.SetMovementEnabled(false);

        if (progressBar != null)
        {
            progressBar.SetImmediateProgress(0f);
            progressBar.Show(false);
        }

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

            if (progressBar != null)
            {
                progressBar.SetProgress(timer / digDuration);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("cancelando cavado");
                CancelDigging();
                yield break;
            }

            yield return null;
        }

        if (plantingSpotPrefab != null)
        {
            if (digSpotsContainer != null)
            {
                GameObject newSpot = Instantiate(plantingSpotPrefab, digPosition, Quaternion.identity, digSpotsContainer.transform);
            }
        }

        CompleteDigging();
    }

    private void CompleteDigging()
    {
        isDigging = false;
        playerController.SetMovementEnabled(true);
        Debug.Log("Proceso de cavado completo");

        if (progressBar != null)
        {
            progressBar.Hide();
        }
    }

    private void CancelDigging()
    {
        isDigging = false;
        playerController.SetMovementEnabled(true);

        if (progressBar != null)
        {
            progressBar.Hide();
        }

        Debug.Log("Cavado cancelado");
    }

    private void CancelCurrentAction()
    {
        if (isHarvesting)
        {
            CancelHarvest();
        }

        if (isDigging)
        {
            CancelDigging();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, digDistance);
    }
}