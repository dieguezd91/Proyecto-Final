using UnityEngine;
using UnityEngine.EventSystems;

public class AbilityButtonTooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private PlayerAbility ability;

    private bool isPointerOver = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        UIEvents.TriggerAbilityTooltipRequested(ability);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        UIEvents.TriggerAbilityTooltipHide();
    }

    private void OnDisable()
    {
        if (isPointerOver)
        {
            UIEvents.TriggerAbilityTooltipHide();
            isPointerOver = false;
        }
    }
}