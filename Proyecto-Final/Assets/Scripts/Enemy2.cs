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
        FindClosestTarget();
    }

    private void FixedUpdate()
    {
        if (currentTarget == null) return;
        if (GetComponent<KnockbackReceiver>()?.IsBeingKnockedBack() == true) return;

        float dist = Vector2.Distance(transform.position, currentTarget.position);
        if (dist > detectRange) return;

        if (dist > shootingRange)
        {
            Vector2 dir = (currentTarget.position - transform.position).normalized;
            Vector2 nextPos = rb.position + dir * speed * Time.fixedDeltaTime;
            rb.MovePosition(nextPos);
            LookDir(currentTarget.position, transform.position);
        }
        else
        {
            LookDir(currentTarget.position, transform.position);
            if (Time.time >= nextTimeToFire)
            {
                Shoot();
                nextTimeToFire = Time.time + 1f / fireRate;
            }
        }
    }

    private void FindClosestTarget()
    {
        float closestScore = Mathf.Infinity;
        Transform best = null;

        // Jugador
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null)
        {
            float d = Vector2.Distance(transform.position, pObj.transform.position);
            float score = d / playerPriority;
            if (d <= detectRange && score < closestScore)
            {
                closestScore = score; best = pObj.transform;
            }
        }

        // Plantas
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
                    closestScore = score; best = col.transform;
                }
            }
        }

        // Home
        GameObject hObj = GameObject.FindGameObjectWithTag("Home");
        if (hObj != null)
        {
            float d = Vector2.Distance(transform.position, hObj.transform.position);
            float score = d / homePriority;
            if (d <= detectRange && score < closestScore)
            {
                closestScore = score; best = hObj.transform;
            }
        }

        currentTarget = best;
    }

    private void Shoot()
    {
        FireBullet b = BulletPool.Instance.GetBullet();
        b.transform.position = firingPoint.position;
        b.transform.rotation = firingPoint.rotation;
        b.SetDirection((currentTarget.position - transform.position).normalized);
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
