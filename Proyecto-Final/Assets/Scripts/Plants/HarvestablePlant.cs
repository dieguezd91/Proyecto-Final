using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// World object the player harvests with the Harvest tool (by day only).
/// Each harvest spends 1 day action and spawns 1 random pickup from the data's loot table.
/// Has a limited number of uses that refill when a new day starts.
/// All tuning, art, particles and sounds live in the HarvestableResourceSO.
/// </summary>
public class HarvestablePlant : MonoBehaviour
{
    private static readonly int IsHarvestingParam = Animator.StringToHash("IsHarvesting");
    private static readonly int IsDepletedParam = Animator.StringToHash("IsDepleted");
    private static readonly int HarvestedParam = Animator.StringToHash("Harvested");
    private static readonly int RegrowParam = Animator.StringToHash("Regrow");

    [Header("DATA")]
    [Tooltip("Defines uses, range, loot, animations, particles and sounds.")]
    [InlineEditor]
    [SerializeField] private HarvestableResourceSO data;

    [Header("REFERENCES")]
    [Tooltip("Trigger circle = harvest range. Its radius is set from the data's Interaction Range.")]
    [SerializeField] private CircleCollider2D rangeTrigger;
    [Tooltip("Clickable area together with the sprite. Usually the solid base collider.")]
    [SerializeField] private Collider2D clickCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("Uses the data's Animator Controller (states: Idle, Harvesting, Harvested, Depleted, Regrow).")]
    [SerializeField] private Animator animator;
    [Tooltip("Where loot and particles spawn. Defaults to this object.")]
    [SerializeField] private Transform effectsAnchor;

    [Header("EVENTS (optional hooks for extra feedback)")]
    public UnityEvent onHarvestStarted;
    public UnityEvent onHarvested;
    public UnityEvent onDepleted;
    public UnityEvent onRefreshed;

    private static readonly List<HarvestablePlant> activePlants = new List<HarvestablePlant>();
    private readonly HashSet<Collider2D> playerCollidersInRange = new HashSet<Collider2D>();

    private PlayerAbilitySystem playerAbilitySystem;
    private GameObject harvestingParticlesInstance;
    private Color originalColor = Color.white;
    private int usesRemaining;
    private bool isBeingHarvested;
    private bool isHovered;

    public HarvestableResourceSO Data => data;
    public int UsesRemaining => usesRemaining;
    public int MaxUses => data != null ? data.MaxUsesPerDay : 0;
    public float InteractionRange => data != null ? data.InteractionRange : 0f;
    private Vector3 EffectsPosition => effectsAnchor != null ? effectsAnchor.position : transform.position;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null && data != null && data.AnimatorController != null)
            animator.runtimeAnimatorController = data.AnimatorController;

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (data == null)
            Debug.LogWarning($"HarvestablePlant '{name}' has no HarvestableResourceSO assigned.", this);

        ApplyRange();
        usesRemaining = MaxUses;
        SetAnimBool(IsDepletedParam, false);
    }

    private void OnEnable() => activePlants.Add(this);

    private void OnDisable()
    {
        activePlants.Remove(this);
        playerCollidersInRange.Clear();
    }

    private void Start()
    {
        DayCycleController.Instance?.OnNewDay.AddListener(HandleNewDay);
    }

    private void OnDestroy()
    {
        if (DayCycleController.Instance != null)
            DayCycleController.Instance.OnNewDay.RemoveListener(HandleNewDay);

        DestroyHarvestingParticles();
    }

    public bool IsReadyToHarvest() => data != null && usesRemaining > 0;

    public bool IsBeingHarvested() => isBeingHarvested;

    public float GetHarvestDuration() => data != null ? data.HarvestDuration : 0f;

    /// <summary>Remaining uses as 0..1 (used by PlantGrowthUI).</summary>
    public float GetTotalProgress() => MaxUses > 0 ? (float)usesRemaining / MaxUses : 0f;

    /// <summary>True while the player's body collider is inside the range trigger circle.</summary>
    public bool IsPlayerInRange
    {
        get
        {
            playerCollidersInRange.RemoveWhere(c => c == null || !c.isActiveAndEnabled);
            return playerCollidersInRange.Count > 0;
        }
    }

    /// <summary>True if the point is over the sprite or the click collider (the range circle does not count).</summary>
    public bool ContainsClickPoint(Vector2 worldPoint)
    {
        if (clickCollider != null && clickCollider.OverlapPoint(worldPoint))
            return true;

        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return false;

        Bounds bounds = spriteRenderer.bounds;
        return worldPoint.x >= bounds.min.x && worldPoint.x <= bounds.max.x &&
               worldPoint.y >= bounds.min.y && worldPoint.y <= bounds.max.y;
    }

    /// <summary>The active harvestable under the point, or null.</summary>
    public static HarvestablePlant FindAtPoint(Vector2 worldPoint)
    {
        foreach (var plant in activePlants)
        {
            if (plant != null && plant.ContainsClickPoint(worldPoint))
                return plant;
        }

        return null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayerBody(other))
            playerCollidersInRange.Add(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        playerCollidersInRange.Remove(other);
    }

    private static bool IsPlayerBody(Collider2D other)
    {
        if (other.isTrigger)
            return false;

        return other.CompareTag("Player") ||
               (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"));
    }

    private void ApplyRange()
    {
        if (rangeTrigger == null)
            return;

        rangeTrigger.isTrigger = true;

        if (data != null)
            rangeTrigger.radius = data.InteractionRange;
    }

#if UNITY_EDITOR
    // Keeps the range circle in sync with the data while editing.
    private void OnValidate() => ApplyRange();
#endif

    /// <summary>Called by PlayerAbilitySystem when the player starts channeling.</summary>
    public void StartHarvest()
    {
        if (!IsReadyToHarvest() || isBeingHarvested)
            return;

        isBeingHarvested = true;

        SetTint(data.HarvestingColor);
        SetAnimBool(IsHarvestingParam, true);

        DestroyHarvestingParticles();
        if (data.HarvestingParticles != null)
            harvestingParticlesInstance = Instantiate(data.HarvestingParticles, EffectsPosition, Quaternion.identity, transform);

        PlaySound(data.HarvestingSound, "Harvest");

        onHarvestStarted?.Invoke();
    }

    public void CancelHarvest()
    {
        if (!isBeingHarvested)
            return;

        isBeingHarvested = false;
        DestroyHarvestingParticles();
        SetTint(originalColor);
        SetAnimBool(IsHarvestingParam, false);
    }

    /// <summary>
    /// Called by PlayerAbilitySystem when the channel finishes.
    /// Spends one use and spawns one pickup. Returns false if nothing was harvested.
    /// </summary>
    public bool CompleteHarvest()
    {
        if (!isBeingHarvested)
            return false;

        isBeingHarvested = false;
        DestroyHarvestingParticles();
        SetTint(isHovered ? data.HighlightColor : originalColor);

        usesRemaining = Mathf.Max(0, usesRemaining - 1);

        SpawnLoot();
        SpawnParticles(data.HarvestedParticles);
        PlaySound(data.HarvestedSound);
        onHarvested?.Invoke();

        bool depleted = usesRemaining <= 0;

        if (depleted)
        {
            SpawnParticles(data.DepletedParticles);
            PlaySound(data.DepletedSound);
            onDepleted?.Invoke();
        }

        SetAnimBool(IsHarvestingParam, false);
        SetAnimBool(IsDepletedParam, depleted);
        SetAnimTrigger(HarvestedParam);
        return true;
    }

    /// <summary>Refills all uses. Called automatically when a new day starts.</summary>
    [ContextMenu("Refresh Uses")]
    public void Refresh()
    {
        if (data == null)
            return;

        bool wasFull = usesRemaining >= MaxUses;

        CancelHarvest();
        usesRemaining = MaxUses;
        SetAnimBool(IsDepletedParam, false);

        if (wasFull)
            return;

        SpawnParticles(data.RegrowParticles);
        PlaySound(data.RegrowSound);
        SetAnimTrigger(RegrowParam);
        onRefreshed?.Invoke();
    }

    private void HandleNewDay(int day) => Refresh();

    private void SetAnimBool(int param, bool value)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetBool(param, value);
    }

    private void SetAnimTrigger(int param)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetTrigger(param);
    }

    private void SpawnLoot()
    {
        GameObject prefab = data.RollLoot();

        if (prefab == null)
        {
            Debug.LogWarning($"'{data.name}' has an empty loot table.", data);
            return;
        }

        GameObject pickup = Instantiate(prefab, EffectsPosition, Quaternion.identity);

        // Same pop-out as LifeController enemy drops.
        if (pickup.TryGetComponent(out Rigidbody2D rb))
            rb.AddForce(Random.insideUnitCircle.normalized * data.LootScatterForce, ForceMode2D.Impulse);
    }

    private void SpawnParticles(GameObject prefab)
    {
        if (prefab == null)
            return;

        GameObject instance = Instantiate(prefab, EffectsPosition, Quaternion.identity);
        Destroy(instance, data.ParticleLifetime);
    }

    private void DestroyHarvestingParticles()
    {
        if (harvestingParticlesInstance != null)
            Destroy(harvestingParticlesInstance);

        harvestingParticlesInstance = null;
    }

    private void PlaySound(SoundClipData sound, string fallbackSoundName = null)
    {
        if (SoundManager.Instance == null)
            return;

        if (sound != null && sound.clips != null && sound.clips.Length > 0)
        {
            SoundManager.Instance.PlayClip(sound, SoundSourceType.Localized, transform);
        }
        else if (!string.IsNullOrEmpty(fallbackSoundName))
        {
            SoundManager.Instance.Play(fallbackSoundName);
        }
    }

    private void SetTint(Color color)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }

    // Hover highlight over the sprite / click collider. Only with the Harvest tool, by day, inside range.
    private void Update()
    {
        if (isBeingHarvested || data == null)
            return;

        if (playerAbilitySystem == null)
            playerAbilitySystem = FindAnyObjectByType<PlayerAbilitySystem>();

        Camera cam = Camera.main;

        bool hovered = cam != null &&
                       playerAbilitySystem != null &&
                       playerAbilitySystem.CurrentAbility == PlayerAbility.Harvesting &&
                       GameFlowController.Instance != null &&
                       GameFlowController.Instance.CurrentPhase == GamePhase.Day &&
                       IsReadyToHarvest() &&
                       IsPlayerInRange &&
                       ContainsClickPoint(cam.ScreenToWorldPoint(Input.mousePosition));

        if (hovered == isHovered)
            return;

        isHovered = hovered;
        SetTint(isHovered ? data.HighlightColor : originalColor);
    }
}
