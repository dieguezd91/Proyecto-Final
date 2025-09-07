using UnityEngine;
using System.Collections;

public class FireBullet : MonoBehaviour
{
    [Header("FireBall Settings")]
    public float minDamage = 20f;
    public float maxDamage = 30f;
    public float lifeTime = 3f;
    public float speed = 10f;

    private Vector2 direction;
    private Rigidbody2D rb;
    private float lifeTimer;

    [Header("FireTrail Settings")]
    public GameObject fireTrailPrefab;
    public float trailSpawnRate = 0.1f;

    private float trailTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        lifeTimer = lifeTime;    
        trailTimer = 0f;         
    }


    void Update()
    {
        // Rotaci�n
        if (rb.velocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Trail
        trailTimer += Time.deltaTime;
        if (trailTimer >= trailSpawnRate)
        {
            SpawnTrail();
            trailTimer = 0f;
        }

        // Caducidad
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            BulletPool.Instance.ReturnBullet(this);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Plant") || collision.CompareTag("Home"))
        {

            var life = collision.GetComponent<LifeController>();
            if (life != null)
            {
                float dmg = Random.Range(minDamage, maxDamage);
                life.TakeDamage(dmg, damageElement: LifeController.DamageElement.Fire);
                if (collision.CompareTag("Player") && GameManager.Instance.uiManager != null)
                {
                    CameraShaker.Instance?.Shake(0.3f, 0.3f);
                }
            }
            else
            {
                var houseLife = collision.GetComponent<HouseLifeController>();
                if (houseLife != null)
                {
                    float dmg = Random.Range(minDamage, maxDamage);
                    houseLife.TakeDamage(dmg);
                }
            }

            BulletPool.Instance.ReturnBullet(this);
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

    void SpawnTrail()
    {
        Instantiate(fireTrailPrefab, transform.position, Quaternion.identity);
    }
}
