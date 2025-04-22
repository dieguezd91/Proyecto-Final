using UnityEngine;

public class Spell : MonoBehaviour
{
    [Header("Spell Settings")]
    public float damage = 20f;
    public float lifeTime = 3f;
    public float speed = 10f;

    private Vector2 direction;
    private Rigidbody2D rb;
    [SerializeField] private GameObject floatingDamagePrefab;
    [SerializeField] private GameObject impactParticlesPrefab;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (rb.velocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            LifeController enemyHealth = collision.GetComponent<LifeController>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);

                if (impactParticlesPrefab != null)
                {
                    Instantiate(impactParticlesPrefab, collision.transform.position, Quaternion.identity);
                }

                GameObject floatingText = Instantiate(floatingDamagePrefab, collision.transform.position + Vector3.up * 0.5f, Quaternion.identity);
                floatingText.GetComponent<FloatingDamageText>().SetText(damage);

                CameraShaker.Instance?.Shake(0.2f, 0.2f);

                KnockbackReceiver knockback = collision.GetComponent<KnockbackReceiver>();
                if (knockback != null)
                {
                    Vector2 knockDir = collision.transform.position - transform.position;
                    knockback.ApplyKnockback(knockDir, 3f);
                }
            }

            Destroy(gameObject);
        }
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }
}