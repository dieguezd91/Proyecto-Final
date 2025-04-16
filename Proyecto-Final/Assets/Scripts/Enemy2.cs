using System.Collections;
using UnityEngine;

public class Enemy2 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator Enemy2Anim;

    [Header("Target Settings")]
    [SerializeField] private float playerPriority = 1.0f;
    [SerializeField] private float plantPriority = 1.2f;
    [SerializeField] private float homePriority = 1.5f;
    [SerializeField] private LayerMask plantLayer;
    private Transform currentTarget;
    private string currentTargetType = "none";

    [Header("Detección de rango")]
    [SerializeField] private float detectRange;
    [SerializeField] private float shootingRange;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 0.3f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firingPoint;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float bulletForce = 5f;
    private float nextTimeToFire = 0f;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (Enemy2Anim == null) Enemy2Anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // Actualizar objetivo cada frame
        FindClosestTarget();
    }

    private void FixedUpdate()
    {
        if (currentTarget == null) return;

        float dist = Vector2.Distance(transform.position, currentTarget.position);
        if (dist > detectRange) return;

        // Si está fuera de rango de disparo, moverse hacia el objetivo
        if (dist > shootingRange)
        {
            Vector2 dir = (currentTarget.position - transform.position).normalized;
            Vector2 nextPos = rb.position + dir * speed * Time.fixedDeltaTime;
            rb.MovePosition(nextPos);
            LookDir(currentTarget.position, transform.position);
        }
        else
        {
            // Dentro de rango de disparo: disparar
            LookDir(currentTarget.position, transform.position);
            if (Time.time >= nextTimeToFire)
            {
                Shoot();
                Debug.Log("Disparando");
                nextTimeToFire = Time.time + 1f / fireRate;
            }
        }
    }

    private void FindClosestTarget()
    {
        float closestScore = Mathf.Infinity;
        Transform best = null;
        string bestType = "none";

        // 1) Jugador
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null)
        {
            float d = Vector2.Distance(transform.position, pObj.transform.position);
            float score = d / playerPriority;
            if (d <= detectRange && score < closestScore)
            {
                closestScore = score;
                best = pObj.transform;
                bestType = "player";
            }
        }

        // 2) Plantas
        Collider2D[] plants = Physics2D.OverlapCircleAll(transform.position, detectRange, plantLayer);
        foreach (var col in plants)
        {
            Plant plant = col.GetComponent<Plant>();
            if (plant != null)
            {
                float d = Vector2.Distance(transform.position, col.transform.position);
                float score = d / plantPriority;
                if (score < closestScore)
                {
                    closestScore = score;
                    best = col.transform;
                    bestType = "plant";
                }
            }
        }

        // 3) Home
        GameObject hObj = GameObject.FindGameObjectWithTag("Home");
        if (hObj != null)
        {
            float d = Vector2.Distance(transform.position, hObj.transform.position);
            float score = d / homePriority;
            if (d <= detectRange && score < closestScore)
            {
                closestScore = score;
                best = hObj.transform;
                bestType = "home";
            }
        }

        currentTarget = best;
        currentTargetType = bestType;
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firingPoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation);
        Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();
        if (rbBullet != null)
        {
            rbBullet.AddForce(firingPoint.up * bulletForce, ForceMode2D.Impulse);
        }
    }

    private void LookDir(Vector2 targetPos, Vector2 currentPos)
    {
        Vector2 lookDir = targetPos - currentPos;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        if (currentTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}
