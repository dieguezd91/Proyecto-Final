using UnityEngine;

public class BasicAreaSpell : Spell
{
    [Header("SETTINGS")]
    [SerializeField] private float speed;
    [SerializeField] private float explosionRadius;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float knockbackForce;

    [Header("VFX")]
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private float explosionEffectDuration;

    private Vector2 direction;
    private bool hasExploded = false;
    private bool isInitialized = false;

    [SerializeField] private float armingTime;
    private float spawnTime;

    public override void Cast(Vector2 castDirection, Vector3 spawnPosition)
    {
        direction = castDirection.normalized;
        transform.position = spawnPosition;
        isInitialized = true;
        spawnTime = Time.time;
    }

    private void Update()
    {
        if (hasExploded || !isInitialized) return;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Time.time - spawnTime < armingTime) return;

        if (collision.CompareTag("Player")) return;

        if (!hasExploded)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);

        if (hitEnemies.Length > 0)
        {
            foreach (Collider2D enemy in hitEnemies)
            {
                ApplyDamage(enemy);
            }
        }

        ShowExplosionEffect();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play("AreaSpellExplosion", SoundSourceType.Localized, transform);
        }

        if (Time.timeScale > 0f && CameraShaker.Instance != null)
        {
            CameraShaker.Instance.Shake(0.5f, 0.4f);
        }

        Destroy(gameObject);
    }

    private void ShowExplosionEffect()
    {
        if (explosionEffectPrefab == null) return;

        GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        float scale = explosionRadius;
        explosion.transform.localScale = Vector3.one * scale;

        Destroy(explosion, explosionEffectDuration);
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}