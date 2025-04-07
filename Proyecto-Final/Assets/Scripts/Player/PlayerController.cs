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

    [Header("Referencias")]
    [SerializeField] private EnemiesSpawner gameStateController;
    [SerializeField] private PlayerAbilitySystem abilitySystem;

    private Vector2 moveInput;
    private bool movementEnabled = true;
    private GameState lastGameState = GameState.None;

    private void Start()
    {
        if (gameStateController == null)
        {
            gameStateController = FindObjectOfType<EnemiesSpawner>();
            if (gameStateController == null)
            {
                Debug.Log("No se encontró el gameStateController");
            }
        }

        if (abilitySystem == null)
        {
            abilitySystem = GetComponent<PlayerAbilitySystem>();
        }

        lastGameState = GameManager.Instance.currentGameState;
    }

    void Update()
    {
        bool isPaused = GameManager.Instance.currentGameState == GameState.Paused || PauseMenu.isGamePaused;

        if (!isPaused)
        {
            movementEnabled = true;
        }

        if (isPaused && movementEnabled)
        {
            movementEnabled = false;
        }

        if (isPaused)
        {
            return;
        }

        lastGameState = GameManager.Instance.currentGameState;

        if (!abilitySystem.IsHarvesting())
        {
            HandleMovement();
        }

        if (gameStateController != null && GameManager.Instance.currentGameState == GameState.Night)
        {
            HandleAttack();
        }
    }

    void HandleMovement()
    {
        if (!movementEnabled)
        {
            return;
        }

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;
        transform.position += (Vector3)moveInput * moveSpeed * Time.deltaTime;
    }

    void HandleAttack()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + spellCooldown;
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

    public void SetMovementEnabled(bool enabled)
    {
        if (!enabled)
        {
            return;
        }

        if (GameManager.Instance.currentGameState == GameState.Paused || PauseMenu.isGamePaused)
        {
            return;
        }

        movementEnabled = enabled;
    }

    public bool IsMovementEnabled()
    {
        return movementEnabled;
    }
}