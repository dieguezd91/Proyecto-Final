using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PauseController pauseController;

    private LifeController lifeController;
    private PlayerRespawnController playerRespawnController;
    private KnockbackReceiver knockbackReceiver;

    private bool tutorialBlocksActions;

    private void Awake() 
    { 
        if (pauseController == null) pauseController = FindObjectOfType<PauseController>(); 
        lifeController = GetComponent<LifeController>(); 
        playerRespawnController = GetComponent<PlayerRespawnController>(); 
        knockbackReceiver = GetComponent<KnockbackReceiver>();
    }

    public void SetTutorialActionBlocked(bool blocked)
    {
        tutorialBlocksActions = blocked;
    }

    public bool CanAct()
    {
        if (lifeController != null && !lifeController.IsAlive())
            return false;

        if (playerRespawnController != null && playerRespawnController.IsRespawning)
            return false;

        if (pauseController != null && pauseController.IsPaused)
            return false;

        if (UIManager.Instance?.Flow != null && UIManager.Instance.Flow.HasOpenModal)
            return false;

        if (GameFlowController.Instance != null && GameFlowController.Instance.CurrentPhase == GamePhase.OnRitual)
            return false;

        if (knockbackReceiver != null && knockbackReceiver.IsBeingKnockedBack())
            return false;

        if (tutorialBlocksActions)
            return false;

        return true;
    }
}