using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gnome Enemy Data", menuName = "Enemy Data/Gnome Enemy")]
public class GardenGnomeEnemyDataSO : EnemyDataSO
{
    [Header("Gnome Behavior")]
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float stopDistance = 0.1f;
    [SerializeField] private float chaseYOffset = 0.5f;

    [Header("Explosion")]
    [SerializeField] private float clingDuration = 2f;
    [SerializeField] private float minExplosionDamage = 25f;
    [SerializeField] private float maxExplosionDamage = 35f;

    public float Acceleration => acceleration;
    public float StopDistance => stopDistance;
    public float ChaseYOffset => chaseYOffset;
    public float ClingDuration => clingDuration;
    public float MinExplosionDamage => minExplosionDamage;
    public float MaxExplosionDamage => maxExplosionDamage;
}
