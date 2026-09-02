using UnityEngine;
using System.Collections;

public class PlayerRespawnController : MonoBehaviour
{
    [SerializeField] private float respawnDelay = 5f;

    private LifeController lifeController;
    private PlayerController playerController;
    private PlayerSpellController playerSpellController;
    private PlayerMovementController playerMovementController;

    public bool IsRespawning { get; private set; }
    public float RespawnDelay => respawnDelay;

    private void Awake()
    {
        lifeController = GetComponent<LifeController>();
        playerController = GetComponent<PlayerController>();
        playerSpellController = GetComponent<PlayerSpellController>();
        playerMovementController = GetComponent<PlayerMovementController>();
    }

    public void BeginRespawn()
    {
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AnimateRespawnRecovery(respawnDelay);
        }

        yield return new WaitForSeconds(0.5f);

        IsRespawning = true;
        if (playerMovementController != null)
        {
            playerMovementController.SetMovementEnabled(true);
        }
        if (playerController != null)
        {
            playerController.SetCanAct(false);
        }

        yield return new WaitForSeconds(respawnDelay - 0.5f);

        IsRespawning = false;

        if (lifeController != null)
        {
            lifeController.ResetLife();
        }

        if (playerMovementController != null)
        {
            playerMovementController.SetMovementEnabled(true);
        }
        if (playerController != null)
        {
            playerController.SetCanAct(true);
        }
    }

    public void OnReviveAnimationEnd()
    {
        if (playerController != null)
        {
            playerController.SetCanAct(true);
            if (playerSpellController != null) playerSpellController.RefreshHandNightness();
        }
    }
}
