using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Slider))]
public class SliderHoverTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("GameObject (Image/Text) that will be enabled while hovering")]
    public GameObject valueDisplayObject;

    [Tooltip("TMP text that will show current/max")]
    public TextMeshProUGUI valueText;

    [Tooltip("Optional: use slider.maxValue, or override here")]
    public float overrideMaxValue = -1f;

    Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        if (valueDisplayObject != null) valueDisplayObject.SetActive(false);
    }

    void Update()
    {
        if (valueText == null || slider == null) return;
        float max = overrideMaxValue > 0 ? overrideMaxValue : slider.maxValue;
        int cur = Mathf.CeilToInt(slider.value);
        int mx = Mathf.CeilToInt(max);
        valueText.text = $"{cur}/{mx}";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (valueDisplayObject != null) valueDisplayObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (valueDisplayObject != null) valueDisplayObject.SetActive(false);
    }
}
