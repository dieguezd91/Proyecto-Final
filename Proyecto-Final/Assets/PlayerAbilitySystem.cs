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
    [SerializeField] private GameObject harvestInProgressIcon; // barra que aparece sobre el jugador durante la cosecha

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
            plantInventory = PlantInventory.Instance;
        }
    }

    private void Start()
    {
        currentAbility = PlayerAbility.Digging;
        OnAbilityChanged?.Invoke(currentAbility);

        if (harvestInProgressIcon != null)
        {
            harvestInProgressIcon.SetActive(false);
        }
    }

    private void Update()
    {
        if (isDigging && progressBar != null && progressBarTarget != null)
        {
            progressBar.transform.position = Camera.main.WorldToScreenPoint(
                progressBarTarget.position + progressBarOffset);
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
        Debug.Log($"Cambiando de {currentAbility} a {ability}");

        if (currentAbility != ability)
        {
            CancelCurrentAction();

            currentAbility = ability;
            Debug.Log($"current ability: {currentAbility}");

            OnAbilityChanged?.Invoke(currentAbility);
        }
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
                    GameObject selectedPlantPrefab = plantInventory.GetSelectedPlantPrefab();
                    if (selectedPlantPrefab != null)
                    {
                        float distance = Vector2.Distance(transform.position, spot.transform.position);
                        if (distance <= interactionDistance)
                        {
                            Debug.Log($"Plantando {plantInventory.GetSelectedPlantName()}");
                            spot.Plant(selectedPlantPrefab);
                        }
                    }
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

            foreach (Collider2D collider in colliders)
            {
                PlantingSpot spot = collider.GetComponent<PlantingSpot>();
                if (spot != null)
                {
                    ResourcePlant plant = spot.GetPlantComponent<ResourcePlant>();
                    if (plant != null && plant.IsReadyToHarvest() && !plant.IsBeingHarvested())
                    {
                        float distance = Vector2.Distance(transform.position, spot.transform.position);
                        if (distance <= interactionDistance)
                        {
                            StartHarvesting(plant);
                            return;
                        }
                    }
                }
            }
        }
    }

    public void StartHarvesting(ResourcePlant plant)
    {
        if (plant == null || !plant.IsReadyToHarvest() || plant.IsBeingHarvested())
            return;

        currentHarvestPlant = plant;
        isHarvesting = true;
        playerController.SetMovementEnabled(false);

        if (harvestInProgressIcon != null)
        {
            harvestInProgressIcon.SetActive(true);
        }

        plant.StartHarvest();

        StartCoroutine(MonitorHarvest());
    }

    private IEnumerator MonitorHarvest()
    {
        while (isHarvesting && currentHarvestPlant != null)
        {
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

        if (harvestInProgressIcon != null)
        {
            harvestInProgressIcon.SetActive(false);
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

        if (harvestInProgressIcon != null)
        {
            harvestInProgressIcon.SetActive(false);
        }

        Debug.Log("Cosecha completada");
        currentHarvestPlant = null;
    }

    private void HandleDigging()
    {
        if (isDigging)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Intentando cavar...");
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, diggableLayer);

            if (hit.collider != null)
            {
                Collider2D[] spots = Physics2D.OverlapCircleAll(mousePos, 0.5f, plantingLayer);

                if (spots.Length > 0)
                {
                    Debug.Log($"Este lugar ya esta cavado");
                    return;
                }

                float distance = Vector2.Distance(transform.position, mousePos);

                if (distance <= digDistance)
                {
                    Debug.Log("Iniciando cavado");
                    StartDigging(mousePos);
                }
                else
                {
                    Debug.Log("El lugar esta demasiado lejos");
                }
            }
            else
            {
                Debug.Log("No se puede cavar aca");
            }
        }
    }

    private void StartDigging(Vector2 position)
    {
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