using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jefe : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float chaseYOffset = 0.5f;

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
        FindPlayer();

        defaultSpeed = moveSpeed;
    }

    private void Update()
    {
        if (isDead || player == null || isAttacking) return;

        direction = player.position - transform.position;
        float distanceToPlayer = direction.magnitude;
        Vector2 targetPosition = (Vector2)player.position + Vector2.down * chaseYOffset;

        LifeController playerLife = player.GetComponent<LifeController>();
        if (playerLife != null && playerLife.IsTargetable())
        {
            if (distanceToPlayer <= detectionRange)
            {
                if (distanceToPlayer > minAttackDistance)
                {
                    MoveTowardsPlayer();
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

    private void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }
    private void MoveTowardsPlayer()
    {
        Vector2 velocity = direction.normalized * moveSpeed;
        rb.velocity = velocity;

        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
        }
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
            if (hit.CompareTag("Player") && !meleeDamagedObjects.Contains(hit.gameObject))
            {
                meleeDamagedObjects.Add(hit.gameObject);
                hit.GetComponent<LifeController>()?.TakeDamage(meleeDamage);
                CameraShaker.Instance?.Shake(0.3f, 0.3f);
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
            if (hit.CompareTag("Player") && !specialDamagedObjects.Contains(hit.gameObject))
            {
                specialDamagedObjects.Add(hit.gameObject);
                hit.GetComponent<LifeController>()?.TakeDamage(specialDamage);
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
