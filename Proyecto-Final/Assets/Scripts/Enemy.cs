using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform home; 

    [Header("Target Settings")]
    public float playerPriority = 1.0f;
    public float plantPriority = 1.2f;
    public float homePriority = 1.5f; 
    public LayerMask plantLayer;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float detectionDistance = 10f;
    public float attackDistance = 1.5f;

    [Header("Combat Settings")]
    public float damage = 10f;
    public float attackCooldown = 1f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.7f;
    [SerializeField] private LayerMask attackableLayers;

    [Header("State")]
    //public bool canAttack = true;
    public bool chasingTarget = false;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Vector2 direction;
    private Transform currentTarget;
    private string currentTargetType = "none";

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        
        GameObject homeObject = GameObject.FindGameObjectWithTag("Home");
        if (homeObject != null)
        {
            home = homeObject.transform;
        }

    }

    void Update()
    {
        FindClosestTarget();

        if (currentTarget != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

            if (distanceToTarget <= detectionDistance)
            {
                chasingTarget = true;
                direction = (currentTarget.position - transform.position).normalized;

                anim.SetBool("isAttacking", distanceToTarget <= attackDistance);
            }
            else
            {
                chasingTarget = false;
                direction = Vector2.zero;
            }
        }
        else
        {
            chasingTarget = false;
            direction = Vector2.zero;
        }
    }

    void FindClosestTarget()
    {
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;
        string targetType = "none";

        
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            float adjustedDistance = distanceToPlayer / playerPriority;

            if (adjustedDistance < closestDistance && distanceToPlayer <= detectionDistance)
            {
                closestDistance = adjustedDistance;
                closestTarget = player;
                targetType = "player";
            }
        }

        
        Collider2D[] nearbyPlants = Physics2D.OverlapCircleAll(transform.position, detectionDistance, plantLayer);

        foreach (Collider2D plantCollider in nearbyPlants)
        {
            Plant plant = plantCollider.GetComponent<Plant>();

            if (plant != null)
            {
                float distanceToPlant = Vector2.Distance(transform.position, plantCollider.transform.position);
                float adjustedDistance = distanceToPlant / plantPriority;

                if (adjustedDistance < closestDistance)
                {
                    closestDistance = adjustedDistance;
                    closestTarget = plantCollider.transform;
                    targetType = "plant";
                }
            }
        }

        
        if (home != null)
        {
            float distanceToHome = Vector2.Distance(transform.position, home.position);
            float adjustedDistance = distanceToHome / homePriority;

            if (adjustedDistance < closestDistance && distanceToHome <= detectionDistance)
            {
                closestDistance = adjustedDistance;
                closestTarget = home;
                targetType = "home";
            }
        }

        currentTarget = closestTarget;
        currentTargetType = targetType;
    }

    void FixedUpdate()
    {
        if (GetComponent<KnockbackReceiver>()?.IsBeingKnockedBack() == true)
            return;

        float distanceToTarget = currentTarget != null ? Vector2.Distance(transform.position, currentTarget.position) : Mathf.Infinity;

        if (chasingTarget && distanceToTarget > attackDistance)
        {
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);

            if (anim != null)
                anim.SetBool("isMoving", true);

            if (direction.x > 0.1f)
                spriteRenderer.flipX = true;
            else if (direction.x < -0.1f)
                spriteRenderer.flipX = false;
        }
        else
        {
            if (anim != null)
                anim.SetBool("isMoving", false);
        }
    }

    public void PerformSwordHit()
    {
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, attackableLayers);

        foreach (Collider2D target in hitTargets)
        {
            LifeController life = target.GetComponent<LifeController>();
            if (life != null)
            {
                life.TakeDamage(damage);

                if (currentTargetType == "player" && GameManager.Instance.uiManager != null)
                {
                    GameManager.Instance.uiManager.ShowDamagedScreen();
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        if (currentTarget != null)
        {
            if (currentTargetType == "player")
                Gizmos.color = Color.blue;
            else if (currentTargetType == "plant")
                Gizmos.color = Color.green;
            else if (currentTargetType == "home")
                Gizmos.color = Color.magenta;

            Gizmos.DrawLine(transform.position, currentTarget.position);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
