using UnityEngine;

public class HouseDoor : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float cooldown = 0.5f;

    [SerializeField] private Collider2D outsideCollider;
    [SerializeField] private Collider2D insideCollider;

    [Header("Spawn Points")]
    [SerializeField] private Transform outsideSpawn;
    [SerializeField] private Transform insideSpawn;

    private WorldTransitionAnimator worldTransition;
    private float lastUseTime;
    private Transform player;
    private bool canEnterAtNight = true;
    private LevelManager levelManager;

    private void Awake()
    {
        worldTransition = FindObjectOfType<WorldTransitionAnimator>();
    }

    private void Start()
    {
        levelManager = LevelManager.Instance;

        if (insideCollider != null) insideCollider.isTrigger = false;
        if (outsideCollider != null) outsideCollider.isTrigger = false;

        if (levelManager != null)
        {
            levelManager.OnGameStateChanged += OnGameStateChanged;
        }

        if (worldTransition != null)
        {
            worldTransition.OnStateChanged += OnWorldStateChanged;
        }

        UpdateDoorColliders();
    }

    private void OnDestroy()
    {
        if (levelManager != null)
        {
            levelManager.OnGameStateChanged -= OnGameStateChanged;
        }

        if (worldTransition != null)
        {
            worldTransition.OnStateChanged -= OnWorldStateChanged;
        }
    }

    private void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.Day || newState == GameState.Digging)
        {
            canEnterAtNight = true;
        }
        else if (newState == GameState.Night)
        {
            canEnterAtNight = false;
        }
    }


    private void OnWorldStateChanged(WorldState newWorldState)
    {
        bool isInInterior = (newWorldState == WorldState.Interior);
        UpdateDoorColliders(isInInterior);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (worldTransition == null || worldTransition.IsTransitioning) return;
        if (Time.time - lastUseTime < cooldown) return;

        player = other.transform;
        bool goingInside = !worldTransition.IsInInterior;

        if (goingInside && !canEnterAtNight)
        {
            return;
        }


        if (goingInside && levelManager.GetCurrentGameState() == GameState.Night)
        {
            canEnterAtNight = false;
        }

        HandleTransition(goingInside);
        lastUseTime = Time.time;
    }

    private void HandleTransition(bool goingInside)
    {
        if (player == null) return;

        if (goingInside && insideSpawn != null)
            player.position = insideSpawn.position;
        else if (!goingInside && outsideSpawn != null)
            player.position = outsideSpawn.position;

        if (goingInside)
        {
            worldTransition.EnterHouse();
            TutorialEvents.InvokeHouseEntered();
        }
        else
            worldTransition.ExitHouse();
    }

    private void UpdateDoorColliders()
    {
        if (worldTransition == null) return;
        UpdateDoorColliders(worldTransition.IsInInterior);
    }

    private void UpdateDoorColliders(bool isInterior)
    {
        if (insideCollider != null) insideCollider.gameObject.SetActive(isInterior);
        if (outsideCollider != null) outsideCollider.gameObject.SetActive(!isInterior);
    }
}