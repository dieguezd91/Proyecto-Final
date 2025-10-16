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

    public bool CanShootNow => Time.time >= rangedData.FireRate;


    protected override EnemyDataSO GetEnemyData() => rangedData;

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

    

    public void PerformAttack()
    {
        if (Time.time < nextTimeToFire) return;

        if (currentTarget == null) return;

        Shoot();
        nextTimeToFire = Time.time + fireRate / fireRate;
    }

    private void Shoot()
    {
        FireBullet bullet = BulletPool.Instance.GetBullet();
        bullet.transform.position = firingPoint.position;
        bullet.transform.rotation = firingPoint.rotation;

        Vector2 adjustedTargetPos = (Vector2)currentTarget.position + Vector2.down * aimYOffset;
        Vector2 direction = (adjustedTargetPos - (Vector2)transform.position).normalized;

        soundBase?.PlaySound(EnemySoundType.Attack, SoundSourceType.Localized, transform);
        bullet.SetDirection(direction);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);
    }

    protected override void ProcessMovement()
    {
    }
}
