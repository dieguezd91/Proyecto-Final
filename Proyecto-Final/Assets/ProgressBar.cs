using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Image fillImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private Color harvestColor = new Color(0.4f, 0.8f, 0.4f);
    [SerializeField] private Color digColor = new Color(0.6f, 0.4f, 0.2f);

    private bool isActive = false;
    private float targetProgress = 0f;
    private float currentProgress = 0f;
    private float smoothVelocity;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (fillImage == null)
            fillImage = transform.Find("Fill").GetComponent<Image>();

        canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        currentProgress = Mathf.SmoothDamp(currentProgress, targetProgress, ref smoothVelocity, 0.1f);
        fillImage.fillAmount = currentProgress;

        if (isActive)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, Time.deltaTime * fadeSpeed);
        }
        else
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);

            if (canvasGroup.alpha < 0.01f)
            {
                currentProgress = 0f;
                targetProgress = 0f;
                fillImage.fillAmount = 0f;
            }
        }
    }

    public void SetProgress(float progress)
    {
        targetProgress = Mathf.Clamp01(progress);
    }

    public void Show(bool isHarvesting)
    {
        isActive = true;
        fillImage.color = isHarvesting ? harvestColor : digColor;
    }

    public void Hide()
    {
        isActive = false;
    }

    public void SetImmediateProgress(float progress)
    {
        currentProgress = Mathf.Clamp01(progress);
        targetProgress = currentProgress;
        fillImage.fillAmount = currentProgress;
    }
}