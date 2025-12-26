using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RitualUIController : UIControllerBase
{
    [Header("Ritual Overlay")]
    [SerializeField] private Image ritualOverlayImage;
    [SerializeField] private Sprite[] ritualMoonPhaseSprites = new Sprite[5];
    [SerializeField] private float overlayFadeDuration = 1.2f;

    private Coroutine ritualOverlayCoroutine;

    protected override void CacheReferences() { }

    protected override void ConfigureInitialState()
    {
        SetupRitualOverlay();
    }

    private void SetupRitualOverlay()
    {
        if (ritualOverlayImage == null)
        {
            return;
        }

        Color color = ritualOverlayImage.color;
        color.a = 0f;
        ritualOverlayImage.color = color;
        ritualOverlayImage.gameObject.SetActive(false);
    }

    public void ShowRitualOverlay()
    {
        if (ritualOverlayImage == null)
        {
            return;
        }

        UpdateRitualSpriteForCurrentMoonPhase();

        if (ritualOverlayCoroutine != null)
        {
            StopCoroutine(ritualOverlayCoroutine);
        }

        if (!ritualOverlayImage.gameObject.activeInHierarchy)
        {
            ritualOverlayImage.gameObject.SetActive(true);
        }

        ritualOverlayCoroutine = StartCoroutine(FadeRitualOverlay(0f, 1f));
    }

    public void HideRitualOverlay()
    {
        if (ritualOverlayImage == null)
        {
            return;
        }

        if (ritualOverlayCoroutine != null)
        {
            StopCoroutine(ritualOverlayCoroutine);
        }

        float currentAlpha = ritualOverlayImage.color.a;

        ritualOverlayCoroutine = StartCoroutine(FadeRitualOverlay(currentAlpha, 0f));
    }

    private void UpdateRitualSpriteForCurrentMoonPhase()
    {
        if (ritualMoonPhaseSprites == null || ritualMoonPhaseSprites.Length != 5)
        {
            return;
        }

        int assignedCount = 0;
        for (int i = 0; i < ritualMoonPhaseSprites.Length; i++)
        {
            if (ritualMoonPhaseSprites[i] != null)
            {
                assignedCount++;
            }
        }

        if (LunarCycleManager.Instance == null)
        {
            return;
        }

        MoonPhase currentPhase = LunarCycleManager.Instance.GetCurrentMoonPhase();
        int phaseIndex = (int)currentPhase;

        if (phaseIndex >= 0 && phaseIndex < ritualMoonPhaseSprites.Length)
        {
            Sprite moonSprite = ritualMoonPhaseSprites[phaseIndex];

            if (moonSprite != null)
            {
                ritualOverlayImage.sprite = moonSprite;

                RectTransform rt = ritualOverlayImage.rectTransform;
            }
        }
    }

    private IEnumerator FadeRitualOverlay(float from, float to)
    {
        if (ritualOverlayImage == null)
        {
            yield break;
        }

        if (to > 0f && !ritualOverlayImage.gameObject.activeInHierarchy)
        {
            ritualOverlayImage.gameObject.SetActive(true);
        }

        float elapsed = 0f;
        Color color = ritualOverlayImage.color;

        while (elapsed < overlayFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / overlayFadeDuration;

            color.a = Mathf.Lerp(from, to, t);
            ritualOverlayImage.color = color;

            yield return null;
        }

        color.a = to;
        ritualOverlayImage.color = color;

        if (Mathf.Approximately(to, 0f))
        {
            ritualOverlayImage.gameObject.SetActive(false);
        }

        ritualOverlayCoroutine = null;
    }

    public void ForceUpdateRitualSprite()
    {
        UpdateRitualSpriteForCurrentMoonPhase();
    }
}