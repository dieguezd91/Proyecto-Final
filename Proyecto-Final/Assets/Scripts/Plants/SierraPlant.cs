using System.Collections;
using UnityEngine;

public class SierraPlant : Plant
{
    [Header("DETECTION")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float detectionRange = 6f;

    [Header("ATTACK RANGE")]
    [SerializeField] private float attachRange = 1.5f;

    [Header("ATTACK")]
    [SerializeField] private float attackDamagePerSecond = 5f;
    [SerializeField] private float attackDuration = 3f;
    [SerializeField] private float attackCooldown = 2f;

    [Header("MOVEMENT")]
    [SerializeField] private float attachSpeed = 12f;
    [SerializeField] private float returnSpeed = 8f;

    [Header("ATTACHMENT")]
    [SerializeField] private string gripPointName = "PlantGripPoint";
    [SerializeField] private Vector3 fallbackAttachOffset = Vector3.zero;

    [Header("VFX")]
    [SerializeField] private ParticleSystem attackParticles;

    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    private Transform currentTarget;
    private LifeController currentTargetLife;

    private bool isAttacking = false;
    private bool isReturning = false;

    private float cooldownTimer = 0f;

    private new void Awake()
    {
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
    }

    protected override void Update()
    {
        base.Update();

        if (!IsFullyGrown())
            return;

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        if (isAttacking || isReturning)
            return;

        DetectAndCheckTarget();
    }

    private void DetectAndCheckTarget()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            detectionRange,
            enemyLayer
        );

        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider2D enemy in enemies)
        {
            if (enemy == null)
                continue;

            LifeController life = enemy.GetComponent<LifeController>();

            if (life == null || !life.IsAlive())
                continue;

            float distance = Vector2.Distance(
                transform.position,
                enemy.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        if (closestEnemy == null)
            return;

        if (closestDistance <= attachRange)
        {
            StartCoroutine(AttackEnemy(closestEnemy));
        }
    }

    private IEnumerator AttackEnemy(Transform target)
    {
        if (target == null)
            yield break;

        LifeController life = target.GetComponent<LifeController>();

        if (life == null || !life.IsAlive())
            yield break;

        isAttacking = true;

        currentTarget = target;
        currentTargetLife = life;

        transform.SetParent(null);

        while (currentTarget != null &&
               currentTargetLife != null &&
               currentTargetLife.IsAlive() &&
               Vector2.Distance(transform.position, currentTarget.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                currentTarget.position,
                attachSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (currentTarget == null ||
            currentTargetLife == null ||
            !currentTargetLife.IsAlive())
        {
            yield return ReturnToPlantPosition();
            FinishAttack();
            yield break;
        }

        AttachToEnemy(currentTarget);

        if (attackParticles != null)
            attackParticles.Play();

        float elapsed = 0f;

        while (elapsed < attackDuration &&
               currentTarget != null &&
               currentTargetLife != null &&
               currentTargetLife.IsAlive())
        {
            float damage =
                attackDamagePerSecond * Time.deltaTime;

            currentTargetLife.TakeDamage(
                damage,
                LifeController.DamageType.DamageOverTime
            );

            elapsed += Time.deltaTime;

            yield return null;
        }

        if (attackParticles != null)
        {
            attackParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        yield return ReturnToPlantPosition();

        FinishAttack();
    }

    private void AttachToEnemy(Transform enemy)
    {
        Transform gripPoint = enemy.Find(gripPointName);

        if (gripPoint != null)
        {
            transform.SetParent(gripPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        else
        {
            transform.SetParent(enemy);
            transform.localPosition = fallbackAttachOffset;
            transform.localRotation = Quaternion.identity;
        }
    }

    private IEnumerator ReturnToPlantPosition()
    {
        isReturning = true;

        transform.SetParent(null);

        Vector3 targetPosition =
            originalParent != null
                ? originalParent.TransformPoint(originalLocalPosition)
                : originalLocalPosition;

        while (Vector2.Distance(transform.position, targetPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                returnSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (originalParent != null)
        {
            transform.SetParent(originalParent);
            transform.localPosition = originalLocalPosition;
            transform.localRotation = originalLocalRotation;
        }
        else
        {
            transform.position = targetPosition;
            transform.rotation = originalLocalRotation;
        }

        isReturning = false;
    }

    private void FinishAttack()
    {
        currentTarget = null;
        currentTargetLife = null;

        cooldownTimer = attackCooldown;
        isAttacking = false;
    }

    protected override void OnDestroy()
    {
        if (attackParticles != null)
        {
            attackParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        base.OnDestroy();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attachRange
        );
    }
}