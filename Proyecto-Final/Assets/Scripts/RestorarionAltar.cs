using UnityEngine;

public class RestorationAltar : MonoBehaviour, IInteractable
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

        UIEvents.TriggerRestorationAltarUIToggle();
    }

    public bool CanInteract()
    {
        return LevelManager.Instance.currentGameState != GameState.Night &&
               !RestorationAltarUIManager.isUIOpen;
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