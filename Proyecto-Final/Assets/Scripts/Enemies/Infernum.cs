using UnityEngine;

public class Infernum : EnemyBase
{
    [Header("Ranged Data")]
    [SerializeField] private RangedEnemyDataSO rangedData;

    [Header("Elemental Type")]
    [SerializeField] private bool isIceEnemy;

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
        Vector2 adjustedTargetPos = (Vector2)currentTarget.position + Vector2.down * aimYOffset;
        Vector2 direction = (adjustedTargetPos - (Vector2)transform.position).normalized;

       
        FireBullet projectile = isIceEnemy ? BulletPool.Instance.GetIceBall() : BulletPool.Instance.GetBullet();

        projectile.transform.position = firingPoint.position;
        projectile.transform.rotation = firingPoint.rotation;
        projectile.SetDirection(direction);

        PlayEnemySound(EnemySoundType.Attack, SoundSourceType.Localized, transform);
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
