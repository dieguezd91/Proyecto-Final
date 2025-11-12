using UnityEngine;

public class TeleportSpell : Spell
{
    [Header("TELEPORT SETTINGS")]
    [SerializeField] private float teleportDistance = 5f;
    [SerializeField] private LayerMask collisionMask;

    [Header("VFX")]
    [SerializeField] private GameObject teleportStartEffectPrefab;
    [SerializeField] private GameObject teleportEndEffectPrefab;
    [SerializeField] private float effectDuration = 0.5f;

    private Transform playerTransform;
    private bool hasExecuted = false;

    public override void Cast(Vector2 direction, Vector3 spawnPosition)
    {
        if (hasExecuted) return;
        hasExecuted = true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Destroy(gameObject);
            return;
        }

        playerTransform = player.transform;
        Vector3 startPosition = playerTransform.position;

        Vector2 teleportDirection = direction.normalized;
        Vector3 targetPosition = startPosition + (Vector3)(teleportDirection * teleportDistance);

        RaycastHit2D hit = Physics2D.Raycast(startPosition, teleportDirection, teleportDistance, collisionMask);

        if (hit.collider != null)
        {
            targetPosition = hit.point - teleportDirection * 0.5f;
        }

        ExecuteTeleport(startPosition, targetPosition);

        Destroy(gameObject, effectDuration);
    }

    private void ExecuteTeleport(Vector3 startPos, Vector3 endPos)
    {
        ShowTeleportEffect(startPos, teleportStartEffectPrefab);

        if (playerTransform != null)
        {
            playerTransform.position = endPos;
        }

        ShowTeleportEffect(endPos, teleportEndEffectPrefab);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play("PlayerTeleport", SoundSourceType.Global, playerTransform);
        }

        if (Time.timeScale > 0f && CameraShaker.Instance != null)
        {
            CameraShaker.Instance.Shake(0.2f, 0.15f);
        }
    }

    private void ShowTeleportEffect(Vector3 position, GameObject effectPrefab)
    {
        if (effectPrefab == null) return;

        GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);
        Destroy(effect, effectDuration);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (playerTransform != null)
        {
            Gizmos.DrawWireSphere(playerTransform.position, teleportDistance);
        }
    }
}