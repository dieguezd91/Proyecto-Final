using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class TutorialUI : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelTransform;
    [SerializeField] private Button skipButton;

    [Header("ANIMATION SETTINGS")]
    [SerializeField] private float fadeSpeed = 0.5f;
    [SerializeField] private float scaleAnimDuration = 0.3f;
    [SerializeField] private float typewriterSpeed = 0.015f;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;

    private bool isVisible = false;
    private Coroutine typewriterCoroutine;
    private Sequence currentSequence;

    private void Awake()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (panelTransform != null)
            panelTransform.localScale = Vector3.zero;

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnSkipButtonPressed);
        }

        tutorialPanel.SetActive(false);
    }

    private void OnSkipButtonPressed()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.SkipTutorial();
        }
    }

    public void ShowStep(TutorialStep step)
    {
        KillAllAnimations();

        isVisible = true;
        tutorialPanel.SetActive(true);

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypewriterEffect(step.instructionText));

        currentSequence = DOTween.Sequence();

        currentSequence.Append(canvasGroup.DOFade(1f, fadeSpeed).SetEase(Ease.OutQuad));
        currentSequence.Join(panelTransform.DOScale(Vector3.one, scaleAnimDuration).SetEase(showEase));

        currentSequence.SetUpdate(true);
    }

    public void HideStep()
    {
        if (!isVisible) return;

        KillAllAnimations();
        isVisible = false;

        currentSequence = DOTween.Sequence();

        currentSequence.Append(panelTransform.DOScale(Vector3.zero, scaleAnimDuration).SetEase(hideEase));
        currentSequence.Join(canvasGroup.DOFade(0f, fadeSpeed).SetEase(Ease.InQuad));
        currentSequence.OnComplete(() => tutorialPanel.SetActive(false));

        currentSequence.SetUpdate(true);
    }

    private IEnumerator TypewriterEffect(string text)
    {
        instructionText.text = "";
        instructionText.maxVisibleCharacters = 0;
        instructionText.text = text;

        int totalCharacters = text.Length;

        for (int i = 0; i <= totalCharacters; i++)
        {
            instructionText.maxVisibleCharacters = i;

            yield return new WaitForSecondsRealtime(typewriterSpeed);
        }

        typewriterCoroutine = null;
    }

    private void KillAllAnimations()
    {
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
            currentSequence = null;
        }

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
    }

    private void OnDisable()
    {
        KillAllAnimations();
    }

    private void OnDestroy()
    {
        KillAllAnimations();

        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(OnSkipButtonPressed);
        }
    }
}