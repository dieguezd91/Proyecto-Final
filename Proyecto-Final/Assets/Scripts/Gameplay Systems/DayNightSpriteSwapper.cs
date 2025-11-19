using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class DayNightSpriteSwapper : MonoBehaviour
{
    [Serializable]
    public class VisualSettings
    {
        public Sprite sprite;
        public Vector3 localPosition = Vector3.zero;
        public Vector3 localScale = Vector3.one;

        [Header("Solo para UI")]
        public Vector2 uiSizeDelta = new Vector2(100, 100);
    }

    [Header("CONFIGURACIÓN")]
    [SerializeField] private bool useCustomTransform = true;
    [Space(10)]
    [SerializeField] private VisualSettings daySettings;
    [SerializeField] private VisualSettings nightSettings;

    [Header("REFERENCES")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image uiImage;

    private RectTransform rectTransform;

    private LevelManager levelManager;
    private GameState lastGameState = GameState.None;
    private Coroutine transitionCoroutine;
    private bool isInitialized = false;
    private bool isUI = false;

    void Start()
    {
        if (uiImage == null) uiImage = GetComponent<Image>();

        if (uiImage != null)
        {
            isUI = true;
            rectTransform = GetComponent<RectTransform>();

            if (daySettings.uiSizeDelta == Vector2.zero) daySettings.uiSizeDelta = rectTransform.sizeDelta;
            if (nightSettings.uiSizeDelta == Vector2.zero) nightSettings.uiSizeDelta = rectTransform.sizeDelta;
        }
        else
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                enabled = false;
                return;
            }
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
        SetVisuals(lastGameState);

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
                SetVisuals(currentState);
            }

            lastGameState = currentState;
        }
    }

    private bool IsNightState(GameState state)
    {
        return state == GameState.Night;
    }

    private void SetVisuals(GameState state)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
            ResetColorAlpha();
        }

        VisualSettings targetSettings = IsNightState(state) ? nightSettings : daySettings;

        if (targetSettings.sprite != null)
        {
            if (isUI && uiImage != null)
            {
                uiImage.sprite = targetSettings.sprite;
            }
            else if (!isUI && spriteRenderer != null)
            {
                spriteRenderer.sprite = targetSettings.sprite;
            }

            if (useCustomTransform)
            {
                if (isUI && rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(targetSettings.localPosition.x, targetSettings.localPosition.y);

                    rectTransform.sizeDelta = targetSettings.uiSizeDelta;
                }
                else
                {
                    transform.localPosition = targetSettings.localPosition;
                }

                transform.localScale = targetSettings.localScale;
            }
        }
    }

    private void ResetColorAlpha()
    {
        if (isUI && uiImage != null)
        {
            Color c = uiImage.color;
            uiImage.color = new Color(c.r, c.g, c.b, 1f);
        }
        else if (!isUI && spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            spriteRenderer.color = new Color(c.r, c.g, c.b, 1f);
        }
    }

    public void ForceUpdate()
    {
        if (levelManager != null) SetVisuals(levelManager.currentGameState);
    }
}