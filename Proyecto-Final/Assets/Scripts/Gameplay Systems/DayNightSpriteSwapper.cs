using UnityEngine;
using System.Collections;

public class DayNightSpriteSwapper : MonoBehaviour
{
    [Header("SPRITES")]
    [SerializeField] private Sprite daySprite;
    [SerializeField] private Sprite nightSprite;

    [Header("REFERENCES")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private LevelManager levelManager;
    private GameState lastGameState = GameState.None;
    private Coroutine transitionCoroutine;
    private bool isInitialized = false;

    void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                enabled = false;
                return;
            }
        }

        if (daySprite == null || nightSprite == null)
        {
            enabled = false;
            return;
        }

        StartCoroutine(InitializeDelayed());
    }

    private IEnumerator InitializeDelayed()
    {
        yield return null;

        levelManager = LevelManager.Instance;
        if (levelManager == null)
        {
            enabled = false;
            yield break;
        }

        lastGameState = levelManager.currentGameState;
        SetSprite(lastGameState);

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized || levelManager == null) return;

        GameState currentState = levelManager.currentGameState;

        if (currentState != lastGameState)
        {
            bool wasNight = IsNightState(lastGameState);
            bool isNight = IsNightState(currentState);

            if (wasNight != isNight)
            {
                SetSprite(currentState);
            }

            lastGameState = currentState;
        }
    }

    private bool IsNightState(GameState state)
    {
        return state == GameState.Night;
    }

    private void SetSprite(GameState state)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
            Color color = spriteRenderer.color;
            spriteRenderer.color = new Color(color.r, color.g, color.b, 1f);
        }

        Sprite targetSprite = IsNightState(state) ? nightSprite : daySprite;

        if (targetSprite != null)
        {
            spriteRenderer.sprite = targetSprite;
        }
    }

    private void OnDestroy()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }
    }

    public void SetDaySprite(Sprite sprite)
    {
        daySprite = sprite;
        if (isInitialized && levelManager != null && !IsNightState(levelManager.currentGameState))
        {
            SetSprite(levelManager.currentGameState);
        }
    }

    public void SetNightSprite(Sprite sprite)
    {
        nightSprite = sprite;
        if (isInitialized && levelManager != null && IsNightState(levelManager.currentGameState))
        {
            SetSprite(levelManager.currentGameState);
        }
    }

    public void ForceUpdate()
    {
        if (levelManager != null)
        {
            SetSprite(levelManager.currentGameState);
        }
    }
}