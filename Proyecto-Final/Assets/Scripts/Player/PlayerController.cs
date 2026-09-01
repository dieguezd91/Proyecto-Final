using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private void Awake() { if (pauseController == null) pauseController = FindObjectOfType<PauseController>(); lifeController = GetComponent<LifeController>(); playerRespawnController = GetComponent<PlayerRespawnController>(); }
    [SerializeField] private PauseController pauseController;
    [SerializeField] private Transform firePoint;

    [Header("INPUT")]
    [SerializeField] private InputReader input;

    

    

    [Header("REFERENCES")]
    [SerializeField] private EnemiesSpawner gameStateController;
    [SerializeField] private PlayerAbilitySystem abilitySystem;

    [Header("MANA SYSTEM")]
    [SerializeField] private ManaSystem manaSystem;

    
    private bool canAct = true;
    private GamePhase lastPhase = GamePhase.None;
    private Animator animator;
    private int lastHorizontalDirection = 0;
    private SpriteRenderer spriteRenderer;
    private bool isWalkingSoundPlaying = false;

    [SerializeField] private Animator handAnimator;
    [SerializeField] private SpriteRenderer handRenderer;
    [SerializeField] private int baseHandSortingOrder = 0;
    [SerializeField] private GameObject handObject;
    private KnockbackReceiver knockbackReceiver;
    private LifeController lifeController;
    private PlayerRespawnController playerRespawnController;

    private PlayerAbilitySystem playerAbilitySystem;
    private PlayerMovementController playerMovementController;

    private SpellType currentSpellType = SpellType.Range;

    private void OnEnable()
    {
        if (input == null) return;

        input.OnPrimaryPressed   += HandleAttack;
        input.OnTeleportPressed  += HandleTeleport;
        input.OnCycleInput       += HandleSpellSwitchInput;
    }

    private void OnDisable()
    {
        if (input == null) return;

        input.OnPrimaryPressed   -= HandleAttack;
        input.OnTeleportPressed  -= HandleTeleport;
        input.OnCycleInput       -= HandleSpellSwitchInput;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        knockbackReceiver = GetComponent<KnockbackReceiver>();
        playerAbilitySystem = GetComponent<PlayerAbilitySystem>();
        playerMovementController = GetComponent<PlayerMovementController>();

        GameFlowController.Instance.OnPhaseChanged += OnPhaseChanged;

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

        OnPhaseChanged(GameFlowController.Instance.CurrentPhase);

        lastPhase = GameFlowController.Instance.CurrentPhase;
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
            GameFlowController.Instance.OnPhaseChanged -= OnPhaseChanged;

        if (input != null)
        {
            input.OnPrimaryPressed   -= HandleAttack;
            input.OnTeleportPressed  -= HandleTeleport;
            input.OnCycleInput       -= HandleSpellSwitchInput;
        }
    }

    private void OnPhaseChanged(GamePhase newPhase)
    {
        bool playerIsAliveAndNotRespawning = (lifeController != null && lifeController.IsAlive() && !(playerRespawnController != null && playerRespawnController.IsRespawning));
        bool gameIsPaused = (GameManager.Instance != null && (pauseController != null && pauseController.IsPaused));

        

        canAct = playerIsAliveAndNotRespawning &&
                 !gameIsPaused &&
                 !(UIManager.Instance?.Flow != null && UIManager.Instance.Flow.HasOpenModal) &&
                 newPhase != GamePhase.OnRitual ;

        bool isNight = newPhase == GamePhase.Night;
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

    

    void Update()
    {
        if (lifeController != null && !lifeController.IsAlive() && !(playerRespawnController != null && playerRespawnController.IsRespawning))
        {
            return;
        }

        if (playerAbilitySystem != null && playerAbilitySystem.IsBusy())
        {
            
            return;
        }

        if (GameFlowController.Instance.CurrentPhase == GamePhase.Night && canAct)
        {
            CheckForSpellTypeChange();
        }
    }

    void FixedUpdate()
    {
        if (animator != null && handRenderer != null)
        {
            float aimY = animator.GetFloat("aimY");
            if (aimY > 0.1f) handRenderer.sortingOrder = baseHandSortingOrder - 1;
            else if (aimY < -0.1f) handRenderer.sortingOrder = baseHandSortingOrder + 1;
            else handRenderer.sortingOrder = baseHandSortingOrder;
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
        if (GameFlowController.Instance.CurrentPhase != GamePhase.Night) return;
        if (!canAct) return;
        if (playerAbilitySystem != null && playerAbilitySystem.IsBusy()) return;

        if (CanCastSpell())
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
        WorldTransitionAnimator worldTransitionCheck = FindObjectOfType<WorldTransitionAnimator>();
        if (worldTransitionCheck != null && worldTransitionCheck.IsInInterior)
        {
            return;
        }

        int selectedSlotIndex = SpellInventory.Instance.GetSelectedSlotIndex();
        SpellSlot selectedSpell = SpellInventory.Instance.GetSelectedSpellSlot();

        manaSystem.UseMana(selectedSpell.manaCost);

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(input != null ? (Vector3)input.MouseScreenPosition : Input.mousePosition);
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
        if (playerMovementController != null) playerMovementController.ApplyAttackMovementPenalty();

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

    

    

    public void SetCanAct(bool value)
    {
        canAct = value;
    }

    public bool CanAct()
    {
        return canAct;
    }

    

    

    public void RefreshHandNightness()
    {
        bool isNight = GameFlowController.Instance.CurrentPhase == GamePhase.Night;
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
        if (playerMovementController != null && !playerMovementController.IsMovementEnabled) return;
        if (playerAbilitySystem != null && playerAbilitySystem.IsBusy()) return;

        TryTeleport();
    }

    private void TryTeleport()
    {
        if (playerAbilitySystem == null) return;

        Vector2 castDirection;
        if (playerMovementController.MoveInput.sqrMagnitude > 0.01f)
        {
            castDirection = playerMovementController.MoveInput.normalized;
        }
        else
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(input != null ? (Vector3)input.MouseScreenPosition : Input.mousePosition);
            mousePos.z = 0f;
            castDirection = (mousePos - transform.position).normalized;
        }

        playerAbilitySystem.TryUseTeleport(castDirection);
    }

    

    

    private void HandleSpellSwitchInput(int direction)
    {
        if (SpellInventory.Instance == null) return;
        if (GameFlowController.Instance.CurrentPhase != GamePhase.Night) return;
        if (!canAct) return;

        SpellInventory.Instance.CycleSpell(direction);
    }

}

