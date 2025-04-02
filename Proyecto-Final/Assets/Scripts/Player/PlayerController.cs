using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // Velocidad de movimiento
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject bulletPrefab; // Prefab de la bala
    [SerializeField] private Transform firePoint; // Punto desde donde se dispara
    [SerializeField] private float bulletSpeed = 10f; // Velocidad de la bala

    [Header("Spell Cooldown")]
    [SerializeField] private float spellCooldown = 0.5f;
    private float nextFireTime = 0f;

    [Header("Plant System")]
    [SerializeField] private GameObject attackPlantPrefab; // Prefab de planta de ataque
    [SerializeField] private GameObject defensePlantPrefab; // Prefab de planta de defensa
    [SerializeField] private GameObject resourcePlantPrefab; // Prefab de planta de recursos
    [SerializeField] private GameObject selectedSeed; // Semilla actualmente seleccionada
    [SerializeField] private LayerMask plantingLayer; // Capa de parcelas
    private int selectedPlantType = 1;

    [Header("Harvest System")]
    [SerializeField] private float interactionDistance = 2f; // Distancia maxima para interactuar con plantas
    [SerializeField] private GameObject harvestInProgressIcon; // barra que aparece sobre el jugador durante la cosecha

    [SerializeField] private EnemiesSpawner gameStateController;

    private Vector2 moveInput;
    private bool isHarvesting = false;
    private ResourcePlant currentHarvestPlant = null;

    private void Start()
    {
        if (gameStateController == null)
        {
            gameStateController = FindObjectOfType<EnemiesSpawner>();
            if (gameStateController == null)
            {
                Debug.Log("No se encontró el WaveSpawner");
            }
        }

        UpdateSelectedPlant();

        if (harvestInProgressIcon != null)
        {
            harvestInProgressIcon.SetActive(false);
        }
    }

    void Update()
    {
        // No permitir movimiento durante la cosecha
        if (!isHarvesting)
        {
            HandleMovement();
        }

        if (gameStateController != null && GameManager.Instance.currentGameState == GameState.Night)
        {
            HandleAttack();
        }

        HandlePlantSelection();

        // Plantar con clic derecho durante el dia
        if (Input.GetMouseButtonDown(1) && gameStateController != null &&
            GameManager.Instance.currentGameState == GameState.Day && !isHarvesting)
        {
            TryPlant();
        }

        // Interactuar con planta cercana con E
        if (Input.GetKeyDown(KeyCode.E) && GameManager.Instance.currentGameState == GameState.Day && !isHarvesting)
        {
            TryInteractWithNearestPlant();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isHarvesting)
        {
            CancelHarvest();
        }
    }

    void HandleMovement()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;
        transform.position += (Vector3)moveInput * moveSpeed * Time.deltaTime;
    }

    void HandleAttack()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime) // Mantener Click izquierdo durante la noche + cooldown
        {
            Shoot();
            nextFireTime = Time.time + spellCooldown;
        }
    }

    void HandlePlantSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedPlantType = 1;
            UpdateSelectedPlant();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedPlantType = 2;
            UpdateSelectedPlant();
        }
    }

    void UpdateSelectedPlant()
    {
        if (selectedPlantType == 1 && attackPlantPrefab != null)
        {
            selectedSeed = attackPlantPrefab;
        }
        else if (selectedPlantType == 2 && defensePlantPrefab != null)
        {
            selectedSeed = defensePlantPrefab;
        }
    }

    void Shoot()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector2 direction = (mousePos - transform.position).normalized;

        GameObject spell = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Spell spellComponent = spell.GetComponent<Spell>();
        if (spellComponent != null)
        {
            spellComponent.SetDirection(direction);
        }
        else
        {
            Rigidbody2D rb = spell.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direction * bulletSpeed;
            }
        }
    }

    void TryPlant()
    {
        GameObject selectedPlantPrefab = PlantInventory.Instance.GetSelectedPlantPrefab();

        if (selectedPlantPrefab == null)
        {
            Debug.Log("No hay planta seleccionada o el slot está vacío");
            return;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, plantingLayer);

        if (hit.collider != null)
        {
            PlantingSpot spot = hit.collider.GetComponent<PlantingSpot>();
            if (spot != null)
            {
                spot.Plant(selectedPlantPrefab);
            }
        }
    }

    void TryInteractWithNearestPlant()
    {
        if (GameManager.Instance.currentGameState != GameState.Day)
        {
            Debug.Log("Solo puedes cosechar plantas durante el día");
            return;
        }

        List<ResourcePlant> harvestablePlants = PlantManager.Instance.GetHarvestablePlants();

        if (harvestablePlants.Count == 0)
        {
            Debug.Log("No hay plantas listas para cosechar");
            return;
        }

        ResourcePlant closestPlant = null;
        float closestDistance = float.MaxValue;

        foreach (ResourcePlant plant in harvestablePlants)
        {
            float distance = Vector2.Distance(transform.position, plant.transform.position);

            if (distance < interactionDistance && distance < closestDistance)
            {
                closestDistance = distance;
                closestPlant = plant;
            }
        }

        if (closestPlant != null)
        {
            StartHarvesting(closestPlant);
        }
    }

    void StartHarvesting(ResourcePlant plant)
    {
        if (plant == null || !plant.IsReadyToHarvest() || plant.IsBeingHarvested())
            return;

        currentHarvestPlant = plant;
        isHarvesting = true;

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

    void CancelHarvest()
    {
        if (!isHarvesting || currentHarvestPlant == null)
            return;

        currentHarvestPlant.CancelHarvest();

        isHarvesting = false;

        if (harvestInProgressIcon != null)
        {
            harvestInProgressIcon.SetActive(false);
        }

        Debug.Log("Cosecha cancelada");
        currentHarvestPlant = null;
    }

    void CompleteHarvest()
    {
        isHarvesting = false;

        if (harvestInProgressIcon != null)
        {
            harvestInProgressIcon.SetActive(false);
        }

        Debug.Log("Cosecha completada");
        currentHarvestPlant = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}