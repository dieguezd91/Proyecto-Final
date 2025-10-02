using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Enemy Data/Base Enemy")]
public class EnemyDataSO : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string enemyName = "Enemy";
    [SerializeField] private Sprite enemyIcon;
    [TextArea(3, 5)]
    [SerializeField] private string description;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 10f;

    [Header("Target Priorities")]
    [SerializeField] private float playerPriority = 1.0f;
    [SerializeField] private float plantPriority = 1.2f;
    [SerializeField] private float homePriority = 1.5f;

    [Header("Audio")]
    [SerializeField] private float footstepCooldown = 0.2f;

    [Header("Rewards")]
    [SerializeField] private int experienceValue = 10;
    [SerializeField] private float manaDropChance = 0.5f;

    public string EnemyName => enemyName;
    public Sprite EnemyIcon => enemyIcon;
    public string Description => description;
    public float MaxHealth => maxHealth;
    public float MoveSpeed => moveSpeed;
    public float DetectionRange => detectionRange;
    public float PlayerPriority => playerPriority;
    public float PlantPriority => plantPriority;
    public float HomePriority => homePriority;
    public float FootstepCooldown => footstepCooldown;
    public int ExperienceValue => experienceValue;
    public float ManaDropChance => manaDropChance;
}