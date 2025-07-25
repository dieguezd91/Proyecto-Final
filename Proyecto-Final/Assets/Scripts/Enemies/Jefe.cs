using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jefe : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float chaseYOffset = 0.5f;

    [Header("Target Settings")]
    [SerializeField] private float playerPriority = 1.0f;
    [SerializeField] private float plantPriority = 1.2f;
    [SerializeField] private float homePriority = 1.5f;
    [SerializeField] private LayerMask plantLayer;

    private Transform currentTarget;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    private float defaultSpeed;

    [Header("Detection Ranges")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float minAttackDistance = 1.5f;
    [SerializeField] private float meleeRadius = 1.2f;
    [SerializeField] private float specialRadius = 2.5f;

    [Header("Damage")]
    [SerializeField] private int meleeDamage = 10;
    [SerializeField] private int specialDamage = 20;

    [Header("Cooldowns & Delays")]
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float meleeDelay = 0.5f;
    [SerializeField] private float specialDelay = 0.7f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform player;
    [SerializeField] private Transform attackPoint;

    [Header("Boss Health")]
    [SerializeField] private float bossMaxHealth = 300f;

    private LifeController lifeController;
    private Vector3 direction;
    private bool isAttacking = false;
    private bool isDead = false;

    private HashSet<GameObject> meleeDamagedObjects = new HashSet<GameObject>();
    private HashSet<GameObject> specialDamagedObjects = new HashSet<GameObject>();


    private void Start()
    {
        Initialize();
        SetupLifeController();

        defaultSpeed = moveSpeed;
    }

    private void Update()
    {
        if (isDead || isAttacking) return;

        FindClosestTarget();

        if (currentTarget == null) return;

        direction = currentTarget.position - transform.position;
        float distanceToTarget = direction.magnitude;
        Vector2 targetPosition = (Vector2)currentTarget.position + Vector2.down * chaseYOffset;

        bool isTargetValid = false;

        if (currentTarget.CompareTag("Player") || currentTarget.CompareTag("Plant"))
        {
            var life = currentTarget.GetComponent<LifeController>();
            if (life != null && life.IsTargetable())
                isTargetValid = true;
        }
        else if (currentTarget.CompareTag("Home"))
        {
            var home = currentTarget.GetComponent<HouseLifeController>();
            if (home != null)
                isTargetValid = true;
        }

        if (isTargetValid)
        {
            if (distanceToTarget <= detectionRange)
            {
                if (distanceToTarget > minAttackDistance)
                {
                    MoveTowardsTarget();
                }
                else
                {
                    rb.velocity = Vector2.zero;
                    TriggerNextAttack();
                }
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }


        FaceDirection(targetPosition - (Vector2)transform.position);
    }

    private void MoveTowardsTarget()
    {
        Vector2 velocity = direction.normalized * moveSpeed;
        rb.velocity = velocity;

        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
        }
    }


    private void Initialize()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        lifeController = GetComponent<LifeController>();
        if (lifeController == null)
        {
            lifeController = gameObject.AddComponent<LifeController>();
        }
    }

    private void SetupLifeController()
    {
        if (lifeController != null)
        {
            lifeController.maxHealth = bossMaxHealth;
            lifeController.currentHealth = bossMaxHealth;

            lifeController.onDeath.AddListener(OnBossDeath);
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
            LifeController playerLife = pObj.GetComponent<LifeController>();
            if (playerLife != null && playerLife.IsTargetable())
            {
                float d = Vector2.Distance(transform.position, pObj.transform.position);
                float score = d / playerPriority;
                if (d <= detectionRange && score < closestScore)
                {
                    closestScore = score;
                    best = pObj.transform;
                }
            }
        }

        // Plantas
        Collider2D[] plants = Physics2D.OverlapCircleAll(transform.position, detectionRange, plantLayer);
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
                }
            }
        }

        // Casa
        GameObject hObj = GameObject.FindGameObjectWithTag("Home");
        if (hObj != null)
        {
            float d = Vector2.Distance(transform.position, hObj.transform.position);
            float score = d / homePriority;
            if (d <= detectionRange && score < closestScore)
            {
                closestScore = score;
                best = hObj.transform;
            }
        }

        currentTarget = best;
    }


    private void TriggerNextAttack()
    {
        if (isAttacking) return;

        isAttacking = true;

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
        }

        int randomAttack = Random.Range(1, 4);

        if (randomAttack == 1)
            StartCoroutine(PerformMeleeAttack());
        else
            StartCoroutine(PerformSpecialAttack());
    }

    private IEnumerator PerformMeleeAttack()
    {
        rb.velocity = Vector2.zero;
        animator.SetTrigger("attack");
        yield return new WaitForSeconds(meleeDelay);

        meleeDamagedObjects.Clear();
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, meleeRadius);

        foreach (var hit in hits)
        {
            if (!meleeDamagedObjects.Contains(hit.gameObject))
            {
                if (hit.CompareTag("Player"))
                {
                    var life = hit.GetComponent<LifeController>();
                    if (life != null)
                    {
                        life.TakeDamage(meleeDamage);
                        CameraShaker.Instance?.Shake(0.3f, 0.3f);
                    }
                }
                else if (hit.CompareTag("Plant"))
                {
                    var life = hit.GetComponent<LifeController>();
                    if (life != null)
                    {
                        life.TakeDamage(meleeDamage);
                    }
                }
                else if (hit.CompareTag("Home"))
                {
                    var home = hit.GetComponent<HouseLifeController>();
                    if (home != null)
                    {
                        home.TakeDamage(meleeDamage);
                    }
                }

                meleeDamagedObjects.Add(hit.gameObject);
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }



    private IEnumerator PerformSpecialAttack()
    {
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(specialDelay);

        specialDamagedObjects.Clear();
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, specialRadius);

        foreach (var hit in hits)
        {
            if (!specialDamagedObjects.Contains(hit.gameObject))
            {
                if (hit.CompareTag("Player"))
                {
                    var life = hit.GetComponent<LifeController>();
                    if (life != null)
                    {
                        life.TakeDamage(specialDamage);
                        CameraShaker.Instance?.Shake(0.5f, 0.5f);
                    }
                }
                else if (hit.CompareTag("Plant"))
                {
                    var life = hit.GetComponent<LifeController>();
                    if (life != null)
                    {
                        life.TakeDamage(specialDamage);
                    }
                }
                else if (hit.CompareTag("Home"))
                {
                    var home = hit.GetComponent<HouseLifeController>();
                    if (home != null)
                    {
                        home.TakeDamage(specialDamage);
                    }
                }

                specialDamagedObjects.Add(hit.gameObject);
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }



    private void FaceDirection(Vector2 lookDir)
    {
        if (lookDir.x > 0.1f)
            spriteRenderer.flipX = false;
        else if (lookDir.x < -0.1f)
            spriteRenderer.flipX = true;
    }

    private void OnBossDeath()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("¡Jefe derrotado!");

        rb.velocity = Vector2.zero;
        isAttacking = true;

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        if (CameraShaker.Instance != null)
        {
            CameraShaker.Instance.Shake(1f, 0.8f);
        }

        yield return new WaitForSeconds(2f);

        Debug.Log("Jefe siendo destruido...");

        Destroy(gameObject);
    }

    public float GetCurrentHealth()
    {
        return lifeController != null ? lifeController.currentHealth : 0f;
    }

    public float GetMaxHealth()
    {
        return lifeController != null ? lifeController.maxHealth : 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minAttackDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoint.position, meleeRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, specialRadius);
    }
}
