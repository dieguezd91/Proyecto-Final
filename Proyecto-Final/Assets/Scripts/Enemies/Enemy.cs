using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IEnemy
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
    public float attackDistance = 1f;

    [Header("Combat Settings")]
    public float minDamage = 8f;
    public float maxDamage = 12f;
    public float attackCooldown = 0.75f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.7f;
    [SerializeField] private LayerMask attackableLayers;

    [Header("State")]
    private bool isCurrentlyAttacking = false;
    public bool chasingTarget = false;
    private bool isDead = false;

    public Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Vector2 direction;
    private Transform currentTarget;
    private string currentTargetType = "none";
    private EnemySoundBase soundBase;
    private LifeController lifeController;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        soundBase = GetComponent<EnemySoundBase>();
        lifeController = GetComponent<LifeController>();
        if (lifeController != null)
        {
            lifeController.onDamaged.AddListener(OnDamaged);
        }
        soundBase?.PlaySound(EnemySoundType.Spawning);

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

    private void OnDamaged(float damage)
    {
        soundBase?.PlaySound(EnemySoundType.Hurt);
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

                if (distanceToTarget <= attackDistance && !isCurrentlyAttacking)
                {
                    anim.SetBool("isAttacking", true);
                    isCurrentlyAttacking = true;
                }
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

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            LifeController playerLife = playerObj.GetComponent<LifeController>();
            if (playerLife != null && playerLife.IsTargetable())
            {
                float distanceToPlayer = Vector2.Distance(transform.position, playerObj.transform.position);
                float adjustedDistance = distanceToPlayer / playerPriority;

                if (adjustedDistance < closestDistance && distanceToPlayer <= detectionDistance)
                {
                    closestDistance = adjustedDistance;
                    closestTarget = playerObj.transform;
                    targetType = "player";
                }
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

        GameObject homeObj = GameObject.FindGameObjectWithTag("Home");
        if (homeObj != null)
        {
            float distanceToHome = Vector2.Distance(transform.position, homeObj.transform.position);
            float adjustedDistance = distanceToHome / homePriority;

            if (adjustedDistance < closestDistance && distanceToHome <= detectionDistance)
            {
                closestDistance = adjustedDistance;
                closestTarget = homeObj.transform;
                targetType = "home";
            }
        }

        currentTarget = closestTarget;
        currentTargetType = targetType;
    }

    void FixedUpdate()
    {
        if (GetComponent<KnockbackReceiver>()?.IsBeingKnockedBack() == true)
        {
            if (anim != null)
                anim.SetBool("isMoving", false);
            return;
        }

        if (isDead) return;

        float distanceToTarget = currentTarget != null ? Vector2.Distance(transform.position, currentTarget.position) : Mathf.Infinity;

        if (chasingTarget && distanceToTarget > attackDistance)
        {
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);

            if (anim != null)
                anim.SetBool("isMoving", true);

            LookAtDirection(direction);
        }
        else
        {
            if (anim != null)
                anim.SetBool("isMoving", false);
        }
    }

    public void PerformSwordHit()
    {
        soundBase?.PlaySound(EnemySoundType.Attack);
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, attackableLayers);
        HashSet<GameObject> damagedTargets = new HashSet<GameObject>();

        foreach (Collider2D target in hitTargets)
        {
            GameObject obj = target.gameObject;

            if (!damagedTargets.Contains(obj))
            {
                damagedTargets.Add(obj);

                LifeController life = obj.GetComponent<LifeController>();
                if (life != null)
                {
                    float dmg = Random.Range(minDamage, maxDamage);
                    life.TakeDamage(dmg);

                    if (currentTargetType == "player" && GameManager.Instance.uiManager != null)
                    {
                        CameraShaker.Instance?.Shake(0.3f, 0.3f);
                    }
                    continue;
                }

                var houseLife = obj.GetComponent<HouseLifeController>();
                if (houseLife != null)
                {
                    float dmg = Random.Range(minDamage, maxDamage);
                    houseLife.TakeDamage(dmg);
                }
            }
        }
    }


    private void LookAtDirection(Vector2 direction)
    {
        if (direction.x > 0.1f)
        {
            spriteRenderer.flipX = true;
            if (attackPoint != null)
                attackPoint.localPosition = new Vector2(Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y);
        }
        else if (direction.x < -0.1f)
        {
            spriteRenderer.flipX = false;
            if (attackPoint != null)
                attackPoint.localPosition = new Vector2(-Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y);
        }
    }

    public void MarkAsDead()
    {
        isDead = true;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        soundBase?.PlaySound(EnemySoundType.Die);
    }

    public void OnAttackAnimationEnd()
    {
        anim.SetBool("isAttacking", false);
        isCurrentlyAttacking = false;
    }

    void OnDestroy()
    {
        if (lifeController != null)
        {
            lifeController.onDamaged.RemoveListener(OnDamaged);
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
