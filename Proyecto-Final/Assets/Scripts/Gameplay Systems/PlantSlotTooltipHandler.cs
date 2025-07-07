using UnityEngine;
using UnityEngine.EventSystems;

public class PlantSlotTooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int slotIndex;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"Pointer entered slot {slotIndex}");
        UIManager.Instance?.ShowTooltipForSlot(slotIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance?.HideTooltip();
    }
}
