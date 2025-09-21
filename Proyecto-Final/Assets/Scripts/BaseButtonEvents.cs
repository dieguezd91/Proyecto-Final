using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseButtonEvents : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
{
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
    }

    public virtual void OnSelect(BaseEventData eventData)
    {
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
    }

    public virtual void OnSubmit(BaseEventData eventData)
    {
    }

    protected virtual void PlayButtonSound(string actionType)
    {
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {

    }
}
