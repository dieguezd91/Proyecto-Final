using UnityEngine;

public class HouseDoor : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    [SerializeField] private float cooldown = 0.5f;
    [SerializeField] private GameObject interactionPromptCanvas;

    [SerializeField] private Collider2D outsideCollider;
    [SerializeField] private Collider2D insideCollider;

    [Header("Spawn Points")]
    [SerializeField] private Transform outsideSpawn;
    [SerializeField] private Transform insideSpawn;

    private WorldTransitionAnimator worldTransition;
    private bool isPlayerNear;
    private float lastUseTime;
    private Transform player;

    private void Awake()
    {
        worldTransition = FindObjectOfType<WorldTransitionAnimator>();
    }

    private void Start()
    {
        if (interactionPromptCanvas != null)
            interactionPromptCanvas.SetActive(false);

        if (insideCollider != null) insideCollider.isTrigger = false;
        if (outsideCollider != null) outsideCollider.isTrigger = false;

        UpdateDoorColliders();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNear = true;
        player = other.transform;
        if (interactionPromptCanvas != null) interactionPromptCanvas.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNear = false;
        if (interactionPromptCanvas != null) interactionPromptCanvas.SetActive(false);
        if (player == other.transform) player = null;
    }

    private void Update()
    {
        if (!isPlayerNear) return;
        if (worldTransition == null || worldTransition.IsTransitioning) return;
        if (Time.time - lastUseTime < cooldown) return;
        if (player == null) return;

        if (Input.GetKeyDown(interactKey))
        {
            bool goingInside = !worldTransition.IsInInterior;

            if (goingInside && insideSpawn != null) player.position = insideSpawn.position;
            else if (!goingInside && outsideSpawn != null) player.position = outsideSpawn.position;

            if (goingInside) worldTransition.EnterHouse();
            else worldTransition.ExitHouse();

            UpdateDoorColliders(goingInside);

            if (interactionPromptCanvas != null) interactionPromptCanvas.SetActive(false);

            lastUseTime = Time.time;
        }
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
