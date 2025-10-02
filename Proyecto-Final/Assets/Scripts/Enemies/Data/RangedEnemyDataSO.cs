using UnityEngine;

[CreateAssetMenu(fileName = "New Ranged Enemy Data", menuName = "Enemy Data/Ranged Enemy")]
public class RangedEnemyDataSO : EnemyDataSO
{
    [Header("Ranged Combat")]
    [SerializeField] private float shootingRange = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float bulletForce = 5f;
    [SerializeField] private float aimYOffset = 0.5f;

    public float ShootingRange => shootingRange;
    public float FireRate => fireRate;
    public float BulletForce => bulletForce;
    public float AimYOffset => aimYOffset;
}