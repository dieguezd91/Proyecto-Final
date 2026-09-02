using UnityEngine;
using System.Collections;

public class PlayerRespawnController : MonoBehaviour
{
    [SerializeField] private float respawnDelay = 5f;

    private LifeController lifeController;
    private PlayerSpellController playerSpellController;
    private PlayerMovementController playerMovementController;

    public bool IsRespawning { get; private set; }
    public float RespawnDelay => respawnDelay;

    private void Awake()
    {
        lifeController = GetComponent<LifeController>();
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
    }
    public void OnReviveAnimationEnd()
    {
        if (playerSpellController != null) playerSpellController.RefreshHandNightness();
    }
}
