using UnityEngine;

public class BasicMeleeSpell : Spell
{
    [Header("SETTINGS")]
    [SerializeField] private float attackRadius = 2f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float knockbackForce = 10f;

    [Header("VFX")]
    [SerializeField] private GameObject slashEffectPrefab;
    [SerializeField] private float effectDuration = 0.5f;

    private Vector2 castDirection;
    private bool hasExecuted = false;

    public override void Cast(Vector2 direction, Vector3 spawnPosition)
    {
        castDirection = direction.normalized;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            transform.position = player.transform.position;
        }
        else
        {
            transform.position = spawnPosition;
        }

        ExecuteMeleeAttack();

        Destroy(gameObject, effectDuration);
    }

    private void ExecuteMeleeAttack()
    {
        if (hasExecuted) return;
        hasExecuted = true;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRadius, enemyLayer);

        if (hitEnemies.Length > 0)
        {
            foreach (Collider2D enemy in hitEnemies)
            {
                ApplyDamage(enemy);
            }
        }

        ShowSlashEffect();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play("PlayerSwordSwing", SoundSourceType.Global, transform);
        }
    }

    private void ShowSlashEffect()
    {
        if (slashEffectPrefab == null) return;

        Vector3 effectPosition = transform.position + (Vector3)castDirection * (attackRadius * 0.5f);

        float angle = Mathf.Atan2(castDirection.y, castDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject effect = Instantiate(slashEffectPrefab, effectPosition, rotation);
        Destroy(effect, effectDuration);
    }

    protected override void ApplyKnockback(Collider2D target)
    {
        var knockback = target.GetComponent<KnockbackReceiver>();
        if (knockback != null)
        {
            Vector2 knockbackDirection = (target.transform.position - transform.position).normalized;
            knockback.ApplyKnockback(knockbackDirection, knockbackForce);
        }
    }

    public void SetDirection(Vector2 newDirection)
    {
        Cast(newDirection, transform.position);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}