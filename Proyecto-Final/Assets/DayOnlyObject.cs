using UnityEngine;

public class DayOnlyObject : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private GameObject targetObject;

    [Header("SETTINGS")]
    [SerializeField] private bool deactivateGameObject = true;

    private Renderer[] renderers;
    private GameState lastGameState = GameState.None;

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
            UpdateVisibility(LevelManager.Instance.GetCurrentGameState());
            lastGameState = LevelManager.Instance.GetCurrentGameState();
        }
    }

    private void Update()
    {
        if (LevelManager.Instance == null) return;

        GameState currentState = LevelManager.Instance.GetCurrentGameState();

        if (currentState != lastGameState)
        {
            UpdateVisibility(currentState);
            lastGameState = currentState;
        }
    }

    private void UpdateVisibility(GameState state)
    {
        bool shouldBeVisible = IsDayState(state);

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

    private bool IsDayState(GameState state)
    {
        return state != GameState.Night && state != GameState.GameOver;
    }
}