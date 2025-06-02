using UnityEngine;
using UnityEngine.UI;

public class PlantGrowthUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);

    [SerializeField] private GameObject promptCanvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private float showDistance = 2.5f;
    private Transform player;

    private ResourcePlant resourcePlant;
    private Camera cam;
    private Canvas canvas;

    private void Start()
    {
        cam = Camera.main;
        resourcePlant = GetComponentInParent<ResourcePlant>();
        canvas = GetComponentInParent<Canvas>();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (resourcePlant == null || fillImage == null || cam == null || promptCanvas == null || player == null)
        {
            Debug.LogWarning("PlantGrowthUI missing references.");
            enabled = false;
            return;
        }

        canvasGroup.alpha = 0f;

        UpdateProgressUI();
    }

    private void LateUpdate()
    {
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            transform.rotation = cam.transform.rotation;
        }

        transform.position = resourcePlant.transform.position + offset;

        float dist = Vector2.Distance(resourcePlant.transform.position, player.position);
        bool shouldShow = dist <= showDistance && !resourcePlant.IsBeingHarvested();

        float targetAlpha = shouldShow ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        canvasGroup.blocksRaycasts = shouldShow;

        if (shouldShow)
        {
            UpdateProgressUI();
        }
    }

    private void UpdateProgressUI()
    {
        float progress = resourcePlant.GetTotalProgress();
        fillImage.fillAmount = progress;

        fillImage.enabled = !resourcePlant.IsBeingHarvested();
    }
}
