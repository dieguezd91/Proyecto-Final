using UnityEngine;
using UnityEngine.UI;

public class PlantGrowthUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Vector3 offset;

    [SerializeField] private GameObject promptCanvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private float showDistance = 2f;

    private Transform player;
    private HarvestablePlant harvestablePlant;

    private Camera cam;
    private Canvas canvas;

    private void Start()
    {
        cam = Camera.main;

        harvestablePlant =
            GetComponentInParent<HarvestablePlant>();

        canvas =
            GetComponentInParent<Canvas>();

        player =
            GameObject.FindGameObjectWithTag("Player")?.transform;

        if (harvestablePlant == null ||
            fillImage == null ||
            cam == null ||
            promptCanvas == null ||
            player == null)
        {
            Debug.LogWarning(
                "PlantGrowthUI missing references."
            );

            enabled = false;
            return;
        }

        canvasGroup.alpha = 0f;

        UpdateProgressUI();
    }

    private void LateUpdate()
    {
        if (canvas != null &&
            canvas.renderMode == RenderMode.WorldSpace)
        {
            transform.rotation = cam.transform.rotation;
        }

        transform.position =
            harvestablePlant.transform.position + offset;

        float dist =
            Vector2.Distance(
                harvestablePlant.transform.position,
                player.position
            );

        bool shouldShow =
            dist <= showDistance &&
            !harvestablePlant.IsBeingHarvested() &&
            GameFlowController.Instance.CurrentPhase != GamePhase.Night &&
            harvestablePlant.IsReadyToHarvest();

        float targetAlpha =
            shouldShow ? 1f : 0f;

        canvasGroup.alpha =
            Mathf.MoveTowards(
                canvasGroup.alpha,
                targetAlpha,
                fadeSpeed * Time.deltaTime
            );

        canvasGroup.blocksRaycasts = shouldShow;

        if (shouldShow)
        {
            UpdateProgressUI();
        }
    }

    public void UpdateProgressUI()
    {
        if (harvestablePlant == null ||
            fillImage == null)
            return;

        float progress =
            harvestablePlant.GetTotalProgress();

        fillImage.fillAmount = progress;

        fillImage.enabled =
            !harvestablePlant.IsBeingHarvested();
    }
}