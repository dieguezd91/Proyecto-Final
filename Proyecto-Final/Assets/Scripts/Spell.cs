using System.Linq;
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
        if (!collision.CompareTag("Enemy"))
            return;

        float dmg = Random.Range(damage, damage + 5f);

        var life = collision.GetComponent<LifeController>();
        life?.TakeDamage(dmg);

        Transform target = collision.transform;
        var existing = target.GetComponentInChildren<FloatingDamageText>();

        if (existing != null)
        {
            existing.AddDamage(dmg);
        }
        else
        {
            Vector3 spawnPos = target.position + Vector3.up * 1f;
            var go = Instantiate(floatingDamagePrefab, spawnPos, Quaternion.identity);
            var fdt = go.GetComponent<FloatingDamageText>();
            fdt.Initialize(target);
            fdt.AddDamage(dmg);
        }

        Destroy(gameObject);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }
}