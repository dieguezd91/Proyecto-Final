using UnityEngine;

[CreateAssetMenu(fileName = "New Boss Data", menuName = "Enemy Data/Boss")]
public class BossEnemyDataSO : EnemyDataSO
{
    [Header("Boss Stats")]
    [SerializeField] private float bossMaxHealth = 300f;

    [Header("Combat")]
    [SerializeField] private float minAttackDistance = 1.5f;
    [SerializeField] private float meleeRadius = 1.2f;
    [SerializeField] private float specialRadius = 2.5f;
    [SerializeField] private int meleeDamage = 10;
    [SerializeField] private int specialDamage = 20;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float meleeDelay = 0.5f;
    [SerializeField] private float specialDelay = 0.7f;

    [Header("Minions")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private int minionCount = 6;
    [SerializeField] private float spawnDelayAfterMinionsDie = 6f;

    public float BossMaxHealth => bossMaxHealth;
    public float MinAttackDistance => minAttackDistance;
    public float MeleeRadius => meleeRadius;
    public float SpecialRadius => specialRadius;
    public int MeleeDamage => meleeDamage;
    public int SpecialDamage => specialDamage;
    public float AttackCooldown => attackCooldown;
    public float MeleeDelay => meleeDelay;
    public float SpecialDelay => specialDelay;
    public GameObject MinionPrefab => minionPrefab;
    public int MinionCount => minionCount;
    public float SpawnDelayAfterMinionsDie => spawnDelayAfterMinionsDie;
}