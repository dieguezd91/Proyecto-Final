using UnityEngine;

public class Infernum : EnemyBase
{
    [Header("Ranged Combat")]
    [SerializeField] private float shootingRange = 5f;
    [SerializeField] private Transform firingPoint;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float aimYOffset = 0.5f;

    private float nextTimeToFire = 0f;

    protected override void ProcessMovement()
    {
        if (currentTarget == null) return;

        float distance = GetDistanceToTarget();
        if (distance > detectionRange) return;

        if (distance > shootingRange)
        {
            Vector2 direction = (currentTarget.position - transform.position).normalized;
            MoveTowardsTarget(direction, moveSpeed);
        }
        else
        {
            StopMovement();
            UpdateSpriteDirection((currentTarget.position - transform.position).normalized);

            if (Time.time >= nextTimeToFire)
            {
                Shoot();
                nextTimeToFire = Time.time + 1f / fireRate;
            }
        }
    }

    private void Shoot()
    {
        FireBullet bullet = BulletPool.Instance.GetBullet();
        bullet.transform.position = firingPoint.position;
        bullet.transform.rotation = firingPoint.rotation;

        Vector2 adjustedTargetPos = (Vector2)currentTarget.position + Vector2.down * aimYOffset;
        Vector2 direction = (adjustedTargetPos - (Vector2)transform.position).normalized;

        soundBase?.PlaySound(EnemySoundType.Attack, EnemySoundBase.SoundSourceType.Localized, transform);
        bullet.SetDirection(direction);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);
    }
}