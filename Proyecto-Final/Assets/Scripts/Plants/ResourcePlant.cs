using System.Collections.Generic;
using UnityEngine;

public class ResourcePlant : Plant
{
    [Header("HEALING AURA")]
    [SerializeField] private float healingRadius = 2.5f;
    [SerializeField] private float healPerSecond = 5f;
    [SerializeField] private LayerMask plantLayer;

    [Header("ENERGY")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float energy = 100f;
    [SerializeField] private float energyConsumptionPerSecond = 10f;
    [SerializeField] private float energyRegenerationPerSecond = 5f;

    [Header("VFX")]
    [SerializeField] private GameObject healingAura;
    [SerializeField] private ParticleSystem healingParticles;

    private bool isHealing = false;

    private readonly HashSet<LifeController> plantsBeingHealed =
        new HashSet<LifeController>();

    public float CurrentEnergy => energy;
    public float MaxEnergy => maxEnergy;
    public bool IsHealing => isHealing;

    protected override void Start()
    {
        base.Start();

        energy = Mathf.Clamp(energy, 0f, maxEnergy);

        if (healingAura != null)
            healingAura.SetActive(false);

        if (healingParticles != null)
            healingParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    protected override void Update()
    {
        base.Update();

        if (!IsFullyGrown())
        {
            StopHealingEffects();
            RegenerateEnergy();
            return;
        }

        plantsBeingHealed.Clear();

        FindPlantsToHeal();

        if (plantsBeingHealed.Count > 0 && energy > 0f)
        {
            HealPlants();
        }
        else
        {
            StopHealingEffects();
            RegenerateEnergy();
        }
    }

    private void FindPlantsToHeal()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            healingRadius,
            plantLayer
        );

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            Plant plant = hit.GetComponentInParent<Plant>();

            if (plant == null || plant == this)
                continue;

            LifeController life = hit.GetComponentInParent<LifeController>();

            if (life == null || !life.IsAlive())
                continue;

            if (life.currentHealth >= life.maxHealth)
                continue;

            plantsBeingHealed.Add(life);
        }
    }

    private void HealPlants()
    {
        if (plantsBeingHealed.Count == 0 || energy <= 0f)
        {
            StopHealingEffects();
            RegenerateEnergy();
            return;
        }

        isHealing = true;

        if (healingAura != null && !healingAura.activeSelf)
            healingAura.SetActive(true);

        if (healingParticles != null && !healingParticles.isPlaying)
            healingParticles.Play();

        float maxEnergyAvailable =
            energyConsumptionPerSecond * Time.deltaTime;

        float energyUsed = Mathf.Min(
            maxEnergyAvailable,
            energy
        );

        float healingMultiplier = energyUsed / maxEnergyAvailable;

        float totalHealing =
            healPerSecond *
            Time.deltaTime *
            healingMultiplier;

        float healingPerPlant =
            totalHealing / plantsBeingHealed.Count;

        foreach (LifeController life in plantsBeingHealed)
        {
            if (life == null || !life.IsAlive())
                continue;

            life.currentHealth = Mathf.Min(
                life.currentHealth + healingPerPlant,
                life.maxHealth
            );

            life.onHealthChanged?.Invoke(
                life.currentHealth,
                life.maxHealth
            );
        }

        energy -= energyUsed;
        energy = Mathf.Clamp(energy, 0f, maxEnergy);

        if (energy <= 0f)
        {
            StopHealingEffects();
        }
    }

    private void RegenerateEnergy()
    {
        if (energy >= maxEnergy)
        {
            energy = maxEnergy;
            return;
        }

        energy += energyRegenerationPerSecond * Time.deltaTime;
        energy = Mathf.Clamp(energy, 0f, maxEnergy);
    }

    private void StopHealingEffects()
    {
        isHealing = false;

        if (healingAura != null)
            healingAura.SetActive(false);

        if (healingParticles != null && healingParticles.isPlaying)
        {
            healingParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }

    protected override void OnMature()
    {
        base.OnMature();

        energy = maxEnergy;
    }

    protected override void HandleGameStateChanged(GamePhase newPhase)
    {
        base.HandleGameStateChanged(newPhase);

        if (newPhase == GamePhase.Night)
        {
            StopHealingEffects();
        }
    }

    protected override void OnDestroy()
    {
        StopHealingEffects();
        base.OnDestroy();
    }

    public float GetEnergyPercent()
    {
        if (maxEnergy <= 0f)
            return 0f;

        return energy / maxEnergy;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isHealing
            ? new Color(0.3f, 1f, 0.5f, 0.6f)
            : new Color(0.3f, 1f, 0.5f, 0.25f);

        Gizmos.DrawWireSphere(
            transform.position,
            healingRadius
        );
    }
}