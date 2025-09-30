using UnityEngine;

public class CraftingCauldron : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        UIEvents.TriggerCraftingUIToggle();
    }

    public bool CanInteract()
    {
        return LevelManager.Instance.currentGameState != GameState.Night &&
               !CraftingUIManager.isCraftingUIOpen;
    }
}