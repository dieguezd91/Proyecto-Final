using UnityEngine;

public class DayOnlyObject : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private GameObject targetObject;

    [Header("SETTINGS")]
    [SerializeField] private bool deactivateGameObject = true;

    private Renderer[] renderers;
    private GamePhase lastPhase = GamePhase.None;

    private void Awake()
    {
        if (targetObject == null)
        {
            targetObject = gameObject;
        }

        if (!deactivateGameObject)
        {
            renderers = targetObject.GetComponentsInChildren<Renderer>(true);
        }
    }

    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            UpdateVisibility(GameFlowController.Instance.CurrentPhase);
            lastPhase = GameFlowController.Instance.CurrentPhase;
        }
    }

    private void Update()
    {
        if (LevelManager.Instance == null) return;

        GamePhase currentPhase = GameFlowController.Instance.CurrentPhase;

        if (currentPhase != lastPhase)
        {
            UpdateVisibility(currentPhase);
            lastPhase = currentPhase;
        }
    }

    private void UpdateVisibility(GamePhase phase)
    {
        bool shouldBeVisible = IsDayPhase(phase);

        if (deactivateGameObject)
        {
            if (targetObject.activeSelf != shouldBeVisible)
            {
                targetObject.SetActive(shouldBeVisible);
            }
        }
        else
        {
            if (renderers != null)
            {
                foreach (Renderer renderer in renderers)
                {
                    if (renderer != null)
                    {
                        renderer.enabled = shouldBeVisible;
                    }
                }
            }
        }
    }

    private bool IsDayPhase(GamePhase phase)
    {
        return phase != GamePhase.Night && phase != GamePhase.GameOver;
    }
}
