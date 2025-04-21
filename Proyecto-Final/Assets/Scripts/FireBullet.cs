using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBullet : MonoBehaviour
{
    [Header("FireBall Settings")]
    public float damage = 20f;
    public float lifeTime = 3f;
    public float speed = 10f;

    private Vector2 direction;
    private Rigidbody2D rb;

    [Header("FireTrail Settings")]
    public GameObject fireTrailPrefab;
    public float trailSpawnRate = 0.1f;

    private float trailTimer;

    [SerializeField] private GameObject DamagedScreen;


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

        trailTimer += Time.deltaTime;
        if (trailTimer >= trailSpawnRate)
        {
            SpawnTrail();
            trailTimer = 0f;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Plant"))
        {
            LifeController Health = collision.GetComponent<LifeController>();
            if (Health != null)
            {
                Health.TakeDamage(damage);
                DamagedScreen.SetActive(true);
                StartCoroutine(DamagedScreenOff());

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


    void SpawnTrail()
    {
        Instantiate(fireTrailPrefab, transform.position, Quaternion.identity);
    }

    IEnumerator DamagedScreenOff()
    {
        yield return new WaitForSeconds(0.5f);
        DamagedScreen.SetActive(false);
    }
}
