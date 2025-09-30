using UnityEngine;

public class RestorationAltar : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        UIEvents.TriggerRestorationAltarUIToggle();
    }

    public bool CanInteract()
    {
        return LevelManager.Instance.currentGameState != GameState.Night &&
               !RestorationAltarUIManager.isUIOpen;
    }
}