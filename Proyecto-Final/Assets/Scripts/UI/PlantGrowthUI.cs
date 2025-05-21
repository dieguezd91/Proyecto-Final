using UnityEngine;
using UnityEngine.UI;

public class PlantGrowthUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);

    private ResourcePlant resourcePlant;
    private Camera cam;
    private Canvas canvas;

    private void Start()
    {
        cam = Camera.main;
        resourcePlant = GetComponentInParent<ResourcePlant>();
        canvas = GetComponentInParent<Canvas>();

        if (resourcePlant == null || fillImage == null || cam == null)
        {
            Debug.LogWarning("PlantGrowthUI missing references or is not on a ResourcePlant.");
            enabled = false;
            return;
        }

        UpdateProgressUI();
    }

    private void LateUpdate()
    {
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            transform.rotation = cam.transform.rotation;
        }

        transform.position = resourcePlant.transform.position + offset;

        UpdateProgressUI();
    }

    private void UpdateProgressUI()
    {
        float progress = resourcePlant.GetTotalProgress();
        fillImage.fillAmount = progress;

        fillImage.enabled = !resourcePlant.IsBeingHarvested();
    }
}
