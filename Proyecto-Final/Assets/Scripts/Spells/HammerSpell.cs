using UnityEngine;

public class HammerSpell : Spell
{
    [Header("SETTINGS")]
    [SerializeField] private float attackRadius = 2f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float knockbackForce = 10f;

    [Header("VFX")]
    [SerializeField] private GameObject slashEffectPrefab;
    [SerializeField] private float effectDuration = 0.5f;

    private bool hasExecuted = false;

    public override void Cast(Vector2 direction, Vector3 spawnPosition)
    {
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

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            transform.position,
            attackRadius,
            enemyLayer
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy == null)
                continue;

            LifeController life = enemy.GetComponent<LifeController>();

            if (life != null && life.IsAlive())
            {
                ApplyDamage(enemy);
            }
        }

        ShowHammerEffect();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play(
                "PlayerSwordSwing",
                SoundSourceType.Global,
                transform
            );
        }
    }

    private void ShowHammerEffect()
    {
        if (slashEffectPrefab == null) return;

        GameObject effect = Instantiate(
            slashEffectPrefab,
            transform.position,
            Quaternion.identity
        );

        Destroy(effect, effectDuration);
    }

    protected override void ApplyKnockback(Collider2D target)
    {
        var knockback = target.GetComponent<KnockbackReceiver>();

        if (knockback != null)
        {
            Vector2 knockbackDirection =
                (target.transform.position - transform.position).normalized;

            knockback.ApplyKnockback(
                knockbackDirection,
                knockbackForce
            );
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