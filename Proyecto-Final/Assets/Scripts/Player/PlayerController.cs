using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 10f;

    [Header("SPELL SETTINGS")]
    [SerializeField] private float spellCooldown = 0.5f;
    private float nextFireTime = 0f;

    [Header("MOVEMENT ACCELERATION")]
    [SerializeField] private float accelerationRate = 8f;
    [SerializeField] private float decelerationRate = 10f;
    [SerializeField] private float maxSpeed = 5f;
    private Vector2 currentVelocity = Vector2.zero;

    [Header("ATTACK MOVEMENT PENALTY")]
    [SerializeField] private float attackMovementPenalty = 0.5f;
    [SerializeField] private float attackSlowDuration = 0.3f;
    private float attackSlowEndTime = 0f;

    [Header("REFERENCES")]
    [SerializeField] private EnemiesSpawner gameStateController;
    [SerializeField] private PlayerAbilitySystem abilitySystem;

    [Header("MANA SYSTEM")]
    [SerializeField] private ManaSystem manaSystem;
    [SerializeField] private float spellManaCost = 15f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashLength = 0.25f;
    [SerializeField] private float dashCooldown = 1f;
    private float moveSpeed;
    private float activeMoveSpeed;


    private float dashCounter;
    private float dashCoolCounter;
    private bool isDashing = false;


    private float lastFootstepTime = 0f;
    private float footstepCooldown = 0.2f;
    [SerializeField] private SurfaceDetector surfaceDetector;

    private Vector2 moveInput;
    private bool movementEnabled = true;
    private bool canAct = true;
    private GameState lastGameState = GameState.None;
    private Animator animator;
    private int lastHorizontalDirection = 0;
    private SpriteRenderer spriteRenderer;
    private bool isWalkingSoundPlaying = false;

    [SerializeField] private Animator handAnimator;
    [SerializeField] private SpriteRenderer handRenderer;
    [SerializeField] private int baseHandSortingOrder = 0;
    [SerializeField] private GameObject handObject;
    private KnockbackReceiver knockbackReceiver;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        knockbackReceiver = GetComponent<KnockbackReceiver>();

        LevelManager.Instance.OnGameStateChanged += HandleGameStateChanged;

        HandleGameStateChanged(LevelManager.Instance.currentGameState);

        moveSpeed = maxSpeed;
        activeMoveSpeed = moveSpeed;

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

        lastGameState = LevelManager.Instance.currentGameState;
    }

    void Update()
    {
        GameState state = LevelManager.Instance.currentGameState;

        var lifeController = GetComponent<LifeController>();
        if (lifeController != null && !lifeController.IsAlive() && !lifeController.isRespawning)
        {
            movementEnabled = false;
            canAct = false;
            return;
        }

        bool inInventory = state == GameState.OnInventory;
        bool inCrafting = state == GameState.OnCrafting;

        if (inInventory || inCrafting)
        {
            movementEnabled = false;
            return;
        }

        movementEnabled = true;

        bool isPaused = state == GameState.Paused || GameManager.Instance.IsGamePaused();
        if (isPaused)
        {
            movementEnabled = false;
            return;
        }

        lastGameState = state;

        if (gameStateController != null && state == GameState.Night && canAct)
        {
            HandleAttack();
        }

        HandleDash();
    }

    void FixedUpdate()
    {
        GameState state = LevelManager.Instance.currentGameState;

        var lifeController = GetComponent<LifeController>();
        if (lifeController != null && !lifeController.IsAlive() && !lifeController.isRespawning)
        {
            currentVelocity = Vector2.zero;
            rb.velocity = Vector2.zero;
            animator.SetBool("IsMoving", false);
            return;
        }

        if (state == GameState.OnInventory || state == GameState.OnCrafting
            || abilitySystem.IsHarvesting() || abilitySystem.IsDigging() ||
            state == GameState.OnRitual || state == GameState.OnAltarRestoration)
        {
            currentVelocity = Vector2.zero;
            rb.velocity = Vector2.zero;
            animator.SetBool("IsMoving", false);
            return;
        }

        if (!movementEnabled)
        {
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, decelerationRate * Time.fixedDeltaTime);
            rb.velocity = currentVelocity;
            animator.SetBool("IsMoving", false);
            return;
        }

        if (knockbackReceiver != null && knockbackReceiver.IsBeingKnockedBack())
        {
            animator.SetBool("IsMoving", false);
            return;
        }

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        Vector2 targetVelocity = moveInput * activeMoveSpeed;

        if (Time.time < attackSlowEndTime)
        {
            float remainingTime = attackSlowEndTime - Time.time;
            float lerpFactor = remainingTime / attackSlowDuration;
            float speedMultiplier = Mathf.Lerp(1f, attackMovementPenalty, lerpFactor);
            targetVelocity *= speedMultiplier;
        }

        if (moveInput.sqrMagnitude > 0.01f)
        {
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, accelerationRate * Time.fixedDeltaTime);
        }
        else
        {
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, decelerationRate * Time.fixedDeltaTime);
        }

        rb.velocity = currentVelocity;

        bool isMoving = currentVelocity.sqrMagnitude > 0.01f;

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
                animator.SetBool("IsMoving", isMoving);
            }
        }

        if (animator != null && handRenderer != null)
        {
            float aimY = animator.GetFloat("aimY");

            if (aimY > 0.1f)
            {
                handRenderer.sortingOrder = baseHandSortingOrder - 1;
            }
            else if (aimY < -0.1f)
            {
                handRenderer.sortingOrder = baseHandSortingOrder + 1;
            }
            else
            {
                handRenderer.sortingOrder = baseHandSortingOrder;
            }
        }
    }

    void HandleAttack()
    {
        if (!canAct) return;

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            handAnimator.SetBool("IsAttacking", true);
        }
    }

    public void OnAttackAnimationEnd()
    {
        handAnimator.SetBool("IsAttacking", false);
    }

    void CastSpell()
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
        SoundManager.Instance.Play("ShootSpell", SoundSourceType.Localized, transform);

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

        attackSlowEndTime = Time.time + attackSlowDuration;
    }

    public void ShootFromHand()
    {
        if (!canAct || Time.time < nextFireTime)
            return;

        CastSpell();
        nextFireTime = Time.time + spellCooldown;
    }

    public void SetMovementEnabled(bool enabled)
    {

        if (enabled && LevelManager.Instance != null && (LevelManager.Instance.currentGameState == GameState.Paused || GameManager.Instance.IsGamePaused()))
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
        if (Time.time - lastFootstepTime >= footstepCooldown)
        {
            string surface = surfaceDetector != null ? surfaceDetector.DetectSurfaceTag() : "Default";

            string soundName;

            switch (surface)
            {
                case "Grass":
                    soundName = "Step_Grass";
                    break;
                case "Land":
                    soundName = "Step_Land";
                    break;
                case "Wood":
                    soundName = "Step_Wood";
                    break;
                default:
                    soundName = "Default";
                    break;
            }

            SoundManager.Instance.Play(soundName, SoundSourceType.Localized, transform);
            lastFootstepTime = Time.time;
        }
    }

    public void ResetAnimator()
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetFloat("aimX", 0f);
            animator.SetFloat("aimY", -1f);
            animator.Play("Idle");
        }
    }

    public void HandleGameStateChanged(GameState newState)
    {
        bool isNight = newState == GameState.Night;

        handAnimator.SetBool("IsNight", isNight);
        if (!isNight)
            handAnimator.SetBool("IsAttacking", false);

        if (handObject != null)
            handObject.SetActive(isNight);
    }

    public void RefreshHandNightness()
    {
        bool isNight = LevelManager.Instance.currentGameState == GameState.Night;
        handAnimator.SetBool("IsNight", isNight);
        if (!isNight)
            handAnimator.SetBool("IsAttacking", false);

        if (handObject != null)
            handObject.SetActive(isNight);
    }


    private void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.Space) && dashCoolCounter <= 0 && dashCounter <= 0)
        {
            activeMoveSpeed = dashSpeed;
            dashCounter = dashLength;
            isDashing = true;
        }

        if (dashCounter > 0)
        {
            dashCounter -= Time.deltaTime;
            if (dashCounter <= 0)
            {
                activeMoveSpeed = moveSpeed;
                dashCoolCounter = dashCooldown;
                isDashing = false;
                rb.velocity = Vector2.zero;
            }
        }

        if (dashCoolCounter > 0)
            dashCoolCounter -= Time.deltaTime;
    }


}