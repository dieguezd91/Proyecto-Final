using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PauseController pauseController;

    private bool canAct = true;
    private LifeController lifeController;
    private PlayerRespawnController playerRespawnController;

    private void Awake() 
    { 
        if (pauseController == null) pauseController = FindObjectOfType<PauseController>(); 
        lifeController = GetComponent<LifeController>(); 
        playerRespawnController = GetComponent<PlayerRespawnController>(); 
    }

    private void Start()
    {
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.OnPhaseChanged += OnPhaseChanged;
            OnPhaseChanged(GameFlowController.Instance.CurrentPhase);
        }
    }

    private void OnDestroy()
    {
        if (GameFlowController.Instance != null)
            GameFlowController.Instance.OnPhaseChanged -= OnPhaseChanged;
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
}
