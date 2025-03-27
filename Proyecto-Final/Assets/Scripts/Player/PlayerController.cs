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
    [SerializeField] private GameObject selectedSeed; // Semilla actualmente seleccionada
    [SerializeField] private LayerMask plantingLayer; // Capa de parcelas
    private int selectedPlantType = 1;

    [SerializeField] private EnemiesSpawner gameStateController;

    private Vector2 moveInput;

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

    }


    void Update()
    {
        HandleMovement();

        if (gameStateController != null && GameManager.Instance.currentGameState == GameState.Night)
        {
            HandleAttack();
        }

        RotatePlayer();

        HandlePlantSelection();

        if (Input.GetMouseButtonDown(1) && gameStateController != null && GameManager.Instance.currentGameState == GameState.Day) //click derecho durante el dia
        {
            TryPlant();
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

    void RotatePlayer()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
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
}
