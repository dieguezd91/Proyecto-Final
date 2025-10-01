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
        return LevelManager.Instance.currentGameState != GameState.Night &&
               !CraftingUIManager.isCraftingUIOpen;
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