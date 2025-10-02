using UnityEngine;

public class Infernum : EnemyBase
{
    [Header("Ranged Data")]
    [SerializeField] private RangedEnemyDataSO rangedData;

    [Header("Combat References")]
    [SerializeField] private Transform firingPoint;

    private float shootingRange;
    private float fireRate;
    private float aimYOffset;
    private float nextTimeToFire = 0f;

    protected override void LoadEnemyData()
    {
        base.LoadEnemyData();

        if (rangedData != null)
        {
            shootingRange = rangedData.ShootingRange;
            fireRate = rangedData.FireRate;
            aimYOffset = rangedData.AimYOffset;
        }
    }

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