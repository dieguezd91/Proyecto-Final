using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PauseController pauseController;
    [Header("INPUT")]
    [SerializeField] private InputReader input;

    private bool canAct = true;
    private LifeController lifeController;
    private PlayerRespawnController playerRespawnController;
    private PlayerAbilitySystem playerAbilitySystem;
    private PlayerMovementController playerMovementController;

    private void Awake() 
    { 
        if (pauseController == null) pauseController = FindObjectOfType<PauseController>(); 
        lifeController = GetComponent<LifeController>(); 
        playerRespawnController = GetComponent<PlayerRespawnController>(); 
    }

    private void Start()
    {
        playerAbilitySystem = GetComponent<PlayerAbilitySystem>();
        playerMovementController = GetComponent<PlayerMovementController>();

        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.OnPhaseChanged += OnPhaseChanged;
            OnPhaseChanged(GameFlowController.Instance.CurrentPhase);
        }
    }

    private void OnEnable()
    {
        if (input == null) return;
        input.OnTeleportPressed += HandleTeleport;
    }

    private void OnDisable()
    {
        if (input == null) return;
        input.OnTeleportPressed -= HandleTeleport;
    }

    private void OnDestroy()
    {
        if (GameFlowController.Instance != null)
            GameFlowController.Instance.OnPhaseChanged -= OnPhaseChanged;

        if (input != null)
        {
            input.OnTeleportPressed -= HandleTeleport;
        }
    }

    private void OnPhaseChanged(GamePhase newPhase)
    {
        bool playerIsAliveAndNotRespawning = (lifeController != null && lifeController.IsAlive() && !(playerRespawnController != null && playerRespawnController.IsRespawning));
        bool gameIsPaused = (pauseController != null && pauseController.IsPaused);

        canAct = playerIsAliveAndNotRespawning &&
                 !gameIsPaused &&
                 !(UIManager.Instance?.Flow != null && UIManager.Instance.Flow.HasOpenModal) &&
                 newPhase != GamePhase.OnRitual;
    }

    public void SetCanAct(bool value)
    {
        canAct = value;
    }

    public bool CanAct()
    {
        return canAct;
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
        if (playerMovementController != null && playerMovementController.MoveInput.sqrMagnitude > 0.01f)
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
}
