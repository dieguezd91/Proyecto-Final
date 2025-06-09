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
    private bool canAct = true;
    private GameState lastGameState = GameState.None;
    private Animator animator;
    private int lastHorizontalDirection = 0;
    private SpriteRenderer spriteRenderer;
    private bool isWalkingSoundPlaying = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

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
        GameState state = GameManager.Instance.currentGameState;
        bool inInventory = state == GameState.OnInventory;
        bool inCrafting = state == GameState.OnCrafting;

        if (inInventory || inCrafting)
        {
            movementEnabled = false;
            return;
        }


        movementEnabled = true;


        bool isPaused = state == GameState.Paused || PauseMenu.isGamePaused;
        if (isPaused)
        {
            movementEnabled = false;
            return;
        }

        lastGameState = state;

        if (gameStateController != null && state == GameState.Night)
        {
            HandleAttack();
        }


    }

    void FixedUpdate()
    {

        
        GameState state = GameManager.Instance.currentGameState;
        if (state == GameState.OnInventory || state == GameState.OnCrafting
            || abilitySystem.IsHarvesting() || abilitySystem.IsDigging())
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("IsMoving", false);
            return;
        }

        if (!movementEnabled)
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("IsMoving", false);

            return;
        }

        if (!movementEnabled || abilitySystem.IsHarvesting() || abilitySystem.IsDigging())
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("IsMoving", false);
            return;
        }

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;


        rb.velocity = moveInput * moveSpeed;


        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        //// ** CONTROL DE SONIDO DE PASOS **
        //if (isMoving && !isWalkingSoundPlaying)
        //{
        //    SoundManager.Instance.PlayLoop("Walk");
        //    isWalkingSoundPlaying = true;
        //}
        //else if (!isMoving && isWalkingSoundPlaying)
        //{
        //    StopWalkingSound();
        //}

        if (animator != null && spriteRenderer != null)
        {
            if (moveInput != Vector2.zero)
            {
                float animAimX = moveInput.x;
                float animAimY = moveInput.y;

                if (moveInput.x > 0.01f)
                {
                    spriteRenderer.flipX = true;
                    animAimX = -moveInput.x;
                }
                else if (moveInput.x < -0.01f)
                {
                    spriteRenderer.flipX = false;
                }

                animator.SetBool("IsMoving", true);
                animator.SetFloat("aimX", animAimX);
                animator.SetFloat("aimY", animAimY);
            }
            else
            {
                animator.SetBool("IsMoving", false);
            }
        }
    }

    void HandleAttack()
    {
        if (!canAct) return;

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + spellCooldown;
        }
    }

    private void StopWalkingSound()
    {
        SoundManager.Instance.Stop("Walk");
        isWalkingSoundPlaying = false;
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
        SoundManager.Instance.PlayOneShot("ShootSpell");
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

        if (enabled && GameManager.Instance != null && (GameManager.Instance.currentGameState == GameState.Paused || PauseMenu.isGamePaused))
        {
            return;
        }

        movementEnabled = enabled;
    }

    public bool IsMovementEnabled()
    {
        return movementEnabled;
    }

    public void SetCanAct(bool value)
    {
        canAct = value;
    }

    public bool CanAct()
    {
        return canAct;
    }

    public void PlayFootstep()
    {
        SoundManager.Instance.PlayOneShot("Walk");
    }

}