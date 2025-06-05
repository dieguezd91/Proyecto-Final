using UnityEngine;

public class Spell : MonoBehaviour
{
    [Header("Spell Settings")]
    public float damage = 20f;
    public float lifeTime = 3f;
    public float speed = 10f;

    private Vector2 direction;
    [SerializeField] private GameObject floatingDamagePrefab;
    [SerializeField] private GameObject impactParticlesPrefab;

    void Awake()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        float r = Random.Range(damage, damage + 5);
        if (collision.gameObject.CompareTag("Enemy"))
        {
            LifeController enemyHealth = collision.GetComponent<LifeController>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(r);

                if (impactParticlesPrefab != null)
                {
                    Instantiate(impactParticlesPrefab, collision.transform.position, Quaternion.identity);
                }

                GameObject floatingText = Instantiate(floatingDamagePrefab, collision.transform.position + Vector3.up * 0.5f, Quaternion.identity);
                floatingText.GetComponent<FloatingDamageText>().SetText(r);

                CameraShaker.Instance?.Shake(0.2f, 0.2f);

                KnockbackReceiver knockback = collision.GetComponent<KnockbackReceiver>();
                if (knockback != null && knockback.gameObject.activeInHierarchy)
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
    }
}