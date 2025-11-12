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

        // Calcular posici�n de destino
        Vector2 teleportDirection = direction.normalized;
        Vector3 targetPosition = startPosition + (Vector3)(teleportDirection * teleportDistance);

        // Verificar si hay colisi�n en el camino
        RaycastHit2D hit = Physics2D.Raycast(startPosition, teleportDirection, teleportDistance, collisionMask);

        if (hit.collider != null)
        {
            // Si hay colisi�n, teleportar justo antes del obst�culo
            targetPosition = hit.point - teleportDirection * 0.5f;
        }

        // Ejecutar teleport
        ExecuteTeleport(startPosition, targetPosition);

        Destroy(gameObject, effectDuration);
    }

    private void ExecuteTeleport(Vector3 startPos, Vector3 endPos)
    {
        // Efecto visual en posici�n inicial
        ShowTeleportEffect(startPos, teleportStartEffectPrefab);

        // Mover al jugador
        if (playerTransform != null)
        {
            playerTransform.position = endPos;
        }

        // Efecto visual en posici�n final
        ShowTeleportEffect(endPos, teleportEndEffectPrefab);

        // Sonido
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play("PlayerTeleport", SoundSourceType.Global, playerTransform);
        }

        // Peque�o screen shake
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