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
    [SerializeField] private GameObject selectedSeed; // Semilla seleccionada
    [SerializeField] private LayerMask plantingLayer; // Capa de parcelas

    [SerializeField] private WaveSpawner gameStateController;

    private Vector2 moveInput;

    private void Start()
    {
        if (gameStateController == null)
        {
            gameStateController = FindObjectOfType<WaveSpawner>();
            if (gameStateController == null)
            {
                Debug.Log("No se encontro el WaveSpawner");
            }
        }
    }


    void Update()
    {
        HandleMovement();

        if (gameStateController != null && gameStateController.currentGameState == GameState.Night)
        {
            HandleAttack();
        }

        RotatePlayer();

        if (Input.GetMouseButtonDown(1) && gameStateController != null && gameStateController.currentGameState == GameState.Day)
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
        if (Input.GetMouseButtonDown(0)) // Clic izquierdo
        {
            Shoot();
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
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, plantingLayer);

        if (hit.collider != null)
        {
            PlantingSpot spot = hit.collider.GetComponent<PlantingSpot>();
            if (spot != null)
            {
                spot.Plant(selectedSeed);
            }
        }
    }

}
