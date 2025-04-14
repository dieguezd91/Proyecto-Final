using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 10f;

    [Header("SPELL SETTINGS")]
    [SerializeField] private float spellCooldown = 0.5f;
    private float nextFireTime = 0f;

    [Header("REFERENCES")]
    [SerializeField] private EnemiesSpawner gameStateController;
    [SerializeField] private PlayerAbilitySystem abilitySystem;

    [Header("MANA SYSTEM")]
    [SerializeField] private ManaSystem manaSystem;
    [SerializeField] private float spellManaCost = 15f;

    private Vector2 moveInput;
    private bool movementEnabled = true;
    private GameState lastGameState = GameState.None;

    private void Start()
    {
        if (gameStateController == null)
        {
            gameStateController = FindObjectOfType<EnemiesSpawner>();
        }

        if (abilitySystem == null)
        {
            abilitySystem = GetComponent<PlayerAbilitySystem>();
        }

        if (manaSystem == null)
        {
            manaSystem = GetComponent<ManaSystem>();
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

        if (gameStateController != null && GameManager.Instance.currentGameState == GameState.Night)
        {
            HandleAttack();
        }
    }

    void FixedUpdate()
    {
        if (!movementEnabled || abilitySystem.IsHarvesting())
        {
            rb.velocity = Vector2.zero;
            return;
        }

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        rb.velocity = moveInput * moveSpeed;
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
        if (!manaSystem.UseMana(spellManaCost))
        {
            return;
        }

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