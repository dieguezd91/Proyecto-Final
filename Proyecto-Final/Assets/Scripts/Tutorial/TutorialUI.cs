using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialUI : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeSpeed = 2f;

    private bool isVisible = false;

    public void ShowStep(TutorialStep step)
    {
        instructionText.text = step.instructionText;

        tutorialPanel.SetActive(true);
        isVisible = true;
        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    public void HideStep()
    {
        isVisible = false;
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    private System.Collections.IEnumerator FadeIn()
    {
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }
    }

    private System.Collections.IEnumerator FadeOut()
    {
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        tutorialPanel.SetActive(false);
    }
}