using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Camera cam;
    [SerializeField] private Transform firePoint;

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

    private PlayerAbilitySystem playerAbilitySystem;
    private bool hasMovedForTutorial = false;

    private SpellType currentSpellType = SpellType.Range;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        knockbackReceiver = GetComponent<KnockbackReceiver>();
        playerAbilitySystem = GetComponent<PlayerAbilitySystem>();

        LevelManager.Instance.OnGameStateChanged += OnGameStateChanged;

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

        OnGameStateChanged(LevelManager.Instance.currentGameState);

        lastGameState = LevelManager.Instance.currentGameState;
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState newState)
    {
        var lifeController = GetComponent<LifeController>();
        bool playerIsAliveAndNotRespawning = (lifeController != null && lifeController.IsAlive() && !lifeController.isRespawning);
        bool gameIsPaused = (GameManager.Instance != null && GameManager.Instance.IsGamePaused());

        movementEnabled = ShouldAllowMovementForState(newState) &&
                          playerIsAliveAndNotRespawning &&
                          !gameIsPaused;

        canAct = playerIsAliveAndNotRespawning &&
                 !gameIsPaused &&
                 newState != GameState.OnInventory &&
                 newState != GameState.OnCrafting &&
                 newState != GameState.OnRitual &&
                 newState != GameState.OnAltarRestoration;

        bool isNight = newState == GameState.Night;
        handAnimator.SetBool("IsNight", isNight);

        if (!isNight)
        {
            handAnimator.SetBool("IsAttacking", false);
            ResetHandSpellAnimations();
        }
        else
        {
            UpdateHandAnimationForSpell();
        }

        if (handObject != null)
            handObject.SetActive(isNight);

        if (!movementEnabled)
        {
            currentVelocity = Vector2.zero;
            rb.velocity = Vector2.zero;
            animator.SetBool("IsMoving", false);
        }
    }

    private bool ShouldAllowMovementForState(GameState state)
    {
        return state == GameState.Day ||
               state == GameState.Digging ||
               state == GameState.Planting ||
               state == GameState.Harvesting ||
               state == GameState.Removing ||
               state == GameState.Night;
    }

    void Update()
    {
        var lifeController = GetComponent<LifeController>();
        if (lifeController != null && !lifeController.IsAlive() && !lifeController.isRespawning)
        {
            return;
        }

        if (playerAbilitySystem != null && playerAbilitySystem.IsBusy())
        {
            rb.velocity = Vector2.zero;

            currentVelocity = Vector2.zero;

            animator.SetBool("IsMoving", false);

            return;
        }

        if (LevelManager.Instance.currentGameState == GameState.Night && canAct)
        {
            CheckForSpellTypeChange();
            HandleAttack();
        }

        HandleTeleport();
    }

    void FixedUpdate()
    {
        GameState state = LevelManager.Instance.currentGameState;

        if (playerAbilitySystem != null && playerAbilitySystem.IsBusy())
        {
            return;
        }

        var lifeController = GetComponent<LifeController>();
        if (lifeController != null && !lifeController.IsAlive() && !lifeController.isRespawning)
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

        // Tutorial movement trigger: only fire the tutorial move event the first time the player moves
        // after the tutorial step has armed it (hasMovedForTutorial == false). External systems (e.g.,
        // TutorialManager) can call ResetHasMovedForTutorial() to re-arm this trigger when showing a
        // Move tutorial step.
        if (!hasMovedForTutorial && moveInput.sqrMagnitude > 0.01f)
        {
            hasMovedForTutorial = true;
            TutorialEvents.InvokePlayerMoved();
        }

        Vector2 targetVelocity = moveInput * maxSpeed;

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

    private void CheckForSpellTypeChange()
    {
        if (SpellInventory.Instance == null || handAnimator == null) return;

        SpellSlot selectedSpell = SpellInventory.Instance.GetSelectedSpellSlot();
        if (selectedSpell == null) return;

        if (selectedSpell.spellType != currentSpellType)
        {
            currentSpellType = selectedSpell.spellType;
            UpdateHandAnimationForSpell();
        }
    }

    void HandleAttack()
    {
        if (!canAct) return;

        if (Input.GetMouseButtonDown(0) && CanCastSpell())
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
        // Prevent casting while inside an interior (house)
        WorldTransitionAnimator worldTransitionCheck = FindObjectOfType<WorldTransitionAnimator>();
        if (worldTransitionCheck != null && worldTransitionCheck.IsInInterior)
        {
            return;
        }

        int selectedSlotIndex = SpellInventory.Instance.GetSelectedSlotIndex();
        SpellSlot selectedSpell = SpellInventory.Instance.GetSelectedSpellSlot();

        manaSystem.UseMana(selectedSpell.manaCost);

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector2 direction = (mousePos - transform.position).normalized;

        GameObject spellObject = Instantiate(selectedSpell.spellPrefab, firePoint.position, Quaternion.identity);

        Spell spellComponent = spellObject.GetComponent<Spell>();

        if (spellComponent != null)
        {
            spellComponent.Cast(direction, firePoint.position);
        }
        else
        {
            Debug.LogWarning($"El prefab {selectedSpell.spellName} no tiene un componente Spell");
            Destroy(spellObject);
        }

        SoundManager.Instance.Play("ShootSpell", SoundSourceType.Localized, transform);
        attackSlowEndTime = Time.time + attackSlowDuration;

        SpellInventory.Instance.StartCooldown(selectedSlotIndex);

        TutorialEvents.InvokeSpellCasted();
    }

    public void ShootFromHand()
    {
        if (!canAct) return;
        if (!CanCastSpell()) return;

        CastSpell();
    }

    private bool CanCastSpell()
    {
        // Block casting if the player is inside a house/interior
        WorldTransitionAnimator worldTransition = FindObjectOfType<WorldTransitionAnimator>();
        if (worldTransition != null && worldTransition.IsInInterior)
            return false;

        if (SpellInventory.Instance == null) return false;

        SpellSlot selectedSpell = SpellInventory.Instance.GetSelectedSpellSlot();

        if (selectedSpell == null || !selectedSpell.isUnlocked) return false;

        if (selectedSpell.currentCooldown > 0f) return false;

        if (manaSystem != null && manaSystem.GetCurrentMana() < selectedSpell.manaCost)
            return false;

        return true;
    }

    private void UpdateHandAnimationForSpell()
    {
        if (SpellInventory.Instance == null || handAnimator == null) return;

        SpellSlot selectedSpell = SpellInventory.Instance.GetSelectedSpellSlot();
        if (selectedSpell == null) return;

        ResetHandSpellAnimations();

        currentSpellType = selectedSpell.spellType;

        switch (selectedSpell.spellType)
        {
            case SpellType.Range:
                handAnimator.SetBool("BaseSpell", true);
                break;

            case SpellType.Melee:
                handAnimator.SetBool("MeleeSpell", true);
                break;

            case SpellType.Area:
                handAnimator.SetBool("AreaSpell", true);
                break;

            case SpellType.Teleport:
                handAnimator.SetBool("BaseSpell", true);
                break;

            default:
                handAnimator.SetBool("BaseSpell", true);
                break;
        }
    }

    private void ResetHandSpellAnimations()
    {
        if (handAnimator == null) return;

        handAnimator.SetBool("BaseSpell", false);
        handAnimator.SetBool("MeleeSpell", false);
        handAnimator.SetBool("AreaSpell", false);
    }

    public void SetMovementEnabled(bool enabled)
    {
        if (enabled && LevelManager.Instance != null && (LevelManager.Instance.currentGameState == GameState.Paused || GameManager.Instance.IsGamePaused()))
        {
            return;
        }

        movementEnabled = enabled;

        if (!enabled)
        {
            canAct = false;
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                currentVelocity = Vector2.zero;
            }
        }
        else
        {
            canAct = true;
        }
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

    public void RefreshHandNightness()
    {
        bool isNight = LevelManager.Instance.currentGameState == GameState.Night;
        handAnimator.SetBool("IsNight", isNight);

        if (!isNight)
        {
            handAnimator.SetBool("IsAttacking", false);
            ResetHandSpellAnimations();
        }
        else
        {
            UpdateHandAnimationForSpell();
        }

        if (handObject != null)
            handObject.SetActive(isNight);
    }

    private void HandleTeleport()
    {
        if (!canAct) return;
        if (!movementEnabled) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CastTeleportSpell();
        }
    }

    private void CastTeleportSpell()
    {
        if (SpellInventory.Instance == null) return;

        WorldTransitionAnimator worldTransition = FindObjectOfType<WorldTransitionAnimator>();
        if (worldTransition != null && worldTransition.IsInInterior)
        {
            return;
        }

        SpellSlot teleportSlot = null;
        int teleportSlotIndex = -1;

        for (int i = 0; i < SpellInventory.Instance.spellSlots.Length; i++)
        {
            if (SpellInventory.Instance.spellSlots[i].spellType == SpellType.Teleport)
            {
                teleportSlot = SpellInventory.Instance.spellSlots[i];
                teleportSlotIndex = i;
                break;
            }
        }

        if (teleportSlot == null || !teleportSlot.isUnlocked) return;
        if (teleportSlot.currentCooldown > 0f) return;
        if (manaSystem != null && manaSystem.GetCurrentMana() < teleportSlot.manaCost) return;
        if (teleportSlot.spellPrefab == null) return;

        manaSystem.UseMana(teleportSlot.manaCost);

        Vector2 castDirection;
        if (moveInput.sqrMagnitude > 0.01f)
        {
            castDirection = moveInput.normalized;
        }
        else
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            castDirection = (mousePos - transform.position).normalized;
        }

        GameObject spellObject = Instantiate(teleportSlot.spellPrefab, transform.position, Quaternion.identity);
        Spell spellComponent = spellObject.GetComponent<Spell>();

        if (spellComponent != null)
        {
            spellComponent.Cast(castDirection, transform.position);
        }
        else
        {
            Destroy(spellObject);
        }

        TutorialEvents.InvokeTeleportCasted();

        SpellInventory.Instance.StartCooldown(teleportSlotIndex);
    }

    // Public API used by tutorial systems ---------------------------------
    // Reset the internal flag so the next player movement will invoke the tutorial move event.
    public void ResetHasMovedForTutorial()
    {
        hasMovedForTutorial = false;
    }

    // Return whether the player is currently moving (useful to immediately trigger move tutorial if already moving)
    public bool IsCurrentlyMoving()
    {
        return rb != null && rb.velocity.sqrMagnitude > 0.01f;
    }
    // ---------------------------------------------------------------------
}