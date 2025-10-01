using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    [SerializeField] private GameObject interactionPromptCanvas;
    [SerializeField] private KeyCode interactionKey = KeyCode.F;
    [SerializeField] private bool hidePromptDuringInteraction = false;

    [Header("Audio")]
    [SerializeField] private SoundClipData interactSound;
    [SerializeField] private SoundClipData completeSound;

    private bool isPlayerNear = false;
    private bool isInteracting = false;
    private IInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<IInteractable>();

        if (interactable == null)
        {
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        UIEvents.OnCraftingUIClosed += OnInteractionComplete;
        UIEvents.OnRestorationAltarUIClosed += OnInteractionComplete;
    }

    private void OnDisable()
    {
        UIEvents.OnCraftingUIClosed -= OnInteractionComplete;
        UIEvents.OnRestorationAltarUIClosed -= OnInteractionComplete;
    }

    private void Start()
    {
        if (interactionPromptCanvas != null)
            interactionPromptCanvas.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerNear &&
            !isInteracting &&
            Input.GetKeyDown(interactionKey) &&
            interactable.CanInteract())
        {
            if (hidePromptDuringInteraction)
            {
                isInteracting = true;
                if (interactionPromptCanvas != null)
                    interactionPromptCanvas.SetActive(false);
            }

            // Play interaction sound globally through SoundManager
            if (interactSound != null && interactSound.CanPlay())
            {
                SoundManager.Instance.PlayClip(interactSound);
                interactSound.SetLastPlayTime();
            }

            interactable.Interact();
        }

        UpdatePromptVisibility();
    }

    private void UpdatePromptVisibility()
    {
        if (interactionPromptCanvas == null) return;

        bool shouldShow = isPlayerNear &&
                         !isInteracting &&
                         interactable.CanInteract();

        interactionPromptCanvas.SetActive(shouldShow);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = true;
            UpdatePromptVisibility();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = false;
            isInteracting = false;
            if (interactionPromptCanvas != null)
                interactionPromptCanvas.SetActive(false);
        }
    }

    public void OnInteractionComplete()
    {
        isInteracting = false;
        UpdatePromptVisibility();

        // Play completion sound globally through SoundManager
        if (completeSound != null && completeSound.CanPlay())
        {
            SoundManager.Instance.PlayClip(completeSound);
            completeSound.SetLastPlayTime();
        }
    }
}