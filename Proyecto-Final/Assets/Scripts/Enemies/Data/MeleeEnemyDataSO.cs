using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Enemy Data", menuName = "Enemy Data/Melee Enemy")]
public class MeleeEnemyDataSO : EnemyDataSO
{
    [Header("Melee Combat")]
    [SerializeField] private float attackDistance = 1f;
    [SerializeField] private float minDamage = 8f;
    [SerializeField] private float maxDamage = 12f;
    [SerializeField] private float attackCooldown = 0.75f;
    [SerializeField] private float attackRange = 0.7f;

    public float AttackDistance => attackDistance;
    public float MinDamage => minDamage;
    public float MaxDamage => maxDamage;
    public float AttackCooldown => attackCooldown;
    public float AttackRange => attackRange;
}