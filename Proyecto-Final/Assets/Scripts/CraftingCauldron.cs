using UnityEngine;

public class CraftingCauldron : MonoBehaviour, IInteractable
{
    [Header("Audio")]
    [SerializeField] private SoundClipData interactSound;
    [SerializeField] private SoundClipData completeSound;

    public void Interact()
    {
        // Play interaction sound globally through SoundManager
        if (interactSound != null && interactSound.CanPlay())
        {
            SoundManager.Instance.PlayClip(interactSound);
            interactSound.SetLastPlayTime();
        }

        UIEvents.TriggerCraftingUIToggle();
    }

    public bool CanInteract()
    {
        return GameFlowController.Instance.CurrentPhase != GamePhase.Night &&
               !(UIManager.Instance?.Flow != null && UIManager.Instance.Flow.IsOpen(UIModal.Crafting));
    }

    public void OnInteractionComplete()
    {
        // Play completion sound globally through SoundManager
        if (completeSound != null && completeSound.CanPlay())
        {
            SoundManager.Instance.PlayClip(completeSound);
            completeSound.SetLastPlayTime();
        }
    }
}
