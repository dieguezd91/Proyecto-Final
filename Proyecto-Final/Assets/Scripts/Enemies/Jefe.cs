using System.Collections;
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

    private Vector3 direction;
    private bool isAttacking = false;

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (attackPoint == null)
            attackPoint = GameObject.FindGameObjectWithTag("AttackPoint")?.transform;

        defaultSpeed = moveSpeed;
    }

    private void Update()
    {
        if (player == null) return;

        direction = player.position - transform.position;
        float distanceToPlayer = direction.magnitude;
        Vector2 targetPosition = (Vector2)player.position + Vector2.down * chaseYOffset;

        LifeController life = player.GetComponent<LifeController>();
        if (life != null && life.IsTargetable())
        {
            if (!isAttacking && distanceToPlayer <= detectionRange)
            {
                if (distanceToPlayer > minAttackDistance)
                    MoveTowardsPlayer();
                else
                {
                    rb.velocity = Vector2.zero;
                    TriggerNextAttack();
                }
            }
            else if (!isAttacking)
            {
                rb.velocity = Vector2.zero;
            }
        }

        FaceDirection(targetPosition - (Vector2)transform.position);
    }

    private void MoveTowardsPlayer()
    {
        Vector2 velocity = direction.normalized * moveSpeed;
        rb.velocity = velocity;
    }

    private void TriggerNextAttack()
    {
        isAttacking = true;
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

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, meleeRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
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

        
        // MainCameraController.Instance.Shake(3f, 1f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, specialRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
                hit.GetComponent<LifeController>()?.TakeDamage(specialDamage);
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
