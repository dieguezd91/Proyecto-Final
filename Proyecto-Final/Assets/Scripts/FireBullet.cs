using UnityEngine;
using System.Collections;

public class FireBullet : MonoBehaviour
{
    [Header("FireBall Settings")]
    public float damage = 20f;
    public float lifeTime = 3f;
    public float speed = 10f;

    private Vector2 direction;
    private Rigidbody2D rb;
    private float lifeTimer;

    [Header("FireTrail Settings")]
    public GameObject fireTrailPrefab;
    public float trailSpawnRate = 0.1f;

    private float trailTimer;

    [SerializeField] private GameObject DamagedScreen;

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
        // Rotación
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
        if (collision.CompareTag("Player") || collision.CompareTag("Plant"))
        {
            var health = collision.GetComponent<LifeController>();
            if (health != null)
            {
                health.TakeDamage(damage);
                if (collision.CompareTag("Player") && GameManager.Instance.uiManager != null)
                    GameManager.Instance.uiManager.ShowDamagedScreen();
            }
            // Al colisión, devolver bala

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

    private void OnDisable()
    {
        Debug.Log($"FireBullet disabled at time {Time.time}", this);
    }
}
