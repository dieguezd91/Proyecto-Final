using System;
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
    [SerializeField] private ScrollRect textScrollRect;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject scrollIndicator;
    [SerializeField] private Image stepImageDisplay;

    [Header("SOUND REFERENCES")]
    [SerializeField] private TutorialSoundBase tutorialSoundBase;
    [SerializeField] private WrittingSoundBase writtingSoundBase;

    [Header("LAYOUT REFERENCES")]
    [SerializeField] private RectTransform textScrollViewport;

    [Header("LAYOUT SETTINGS - NO IMAGE (Full Width)")]
    [SerializeField] private float sizeDeltaX_NoImage = -50f;
    [SerializeField] private float anchoredPosX_NoImage = 0f;

    [Header("LAYOUT SETTINGS - WITH IMAGE (Reduced)")]
    [SerializeField] private float sizeDeltaX_WithImage = -230f;
    [SerializeField] private float anchoredPosX_WithImage = -80f;

    [Header("ANIMATION SETTINGS")]
    [SerializeField] private float fadeSpeed = 0.5f;
    [SerializeField] private float scaleAnimDuration = 0.3f;
    [SerializeField] private float typewriterSpeed = 0.015f;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;
    [SerializeField] private float blinkFadeTime = 0.6f;

    [Header("TYPEWRITER SETTINGS")]
    [SerializeField][Range(1, 8)] private int keypressesPerSound = 1;

    private bool isVisible = false;
    private Coroutine typewriterCoroutine;
    private Sequence currentSequence;
    private Sequence blinkSequence;
    private CanvasGroup indicatorCanvasGroup;

    private int keypressCounter = 0;

    private void Awake()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (panelTransform != null) panelTransform.localScale = Vector3.zero;
        if (stepImageDisplay != null) stepImageDisplay.gameObject.SetActive(false);

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonPressed);
            continueButton.gameObject.SetActive(false);
        }

        if (textScrollRect != null) textScrollRect.onValueChanged.AddListener(OnScrollValueChanged);

        if (scrollIndicator != null)
        {
            indicatorCanvasGroup = scrollIndicator.GetComponent<CanvasGroup>();
            scrollIndicator.SetActive(false);
        }

        if (tutorialSoundBase == null) TryGetComponent(out tutorialSoundBase);
        if (writtingSoundBase == null) TryGetComponent(out writtingSoundBase);

        tutorialPanel.SetActive(false);
    }

    private void OnContinueButtonPressed()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ConfirmWaitStep();
        }
    }

    public void ShowStep(TutorialStep step)
    {
        TutorialSoundBase soundController = FindObjectOfType<TutorialSoundBase>();
        if (soundController != null) tutorialSoundBase.PlaySound(TutorialSoundType.ShowPanel);

        KillAllAnimations();
        isVisible = true;
        tutorialPanel.SetActive(true);

        bool hasImage = step.stepImage != null && stepImageDisplay != null;

        if (stepImageDisplay != null)
        {
            stepImageDisplay.gameObject.SetActive(hasImage);
            if (hasImage)
            {
                stepImageDisplay.sprite = step.stepImage;
                stepImageDisplay.preserveAspect = true;
            }
        }

        if (textScrollViewport != null)
        {
            float targetSizeDeltaX = hasImage ? sizeDeltaX_WithImage : sizeDeltaX_NoImage;
            float targetPosX = hasImage ? anchoredPosX_WithImage : anchoredPosX_NoImage;

            Vector2 size = textScrollViewport.sizeDelta;
            size.x = targetSizeDeltaX;
            textScrollViewport.sizeDelta = size;

            Vector2 pos = textScrollViewport.anchoredPosition;
            pos.x = targetPosX;
            textScrollViewport.anchoredPosition = pos;
        }

        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterEffect(step.instructionText));

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(step.objectiveType == TutorialObjectiveType.Wait);
        }

        currentSequence = DOTween.Sequence();
        currentSequence.Append(canvasGroup.DOFade(1f, fadeSpeed).SetEase(Ease.OutQuad));
        currentSequence.Join(panelTransform.DOScale(Vector3.one, scaleAnimDuration).SetEase(showEase));
        currentSequence.SetUpdate(true);
    }

    public void HideStep()
    {
        if (!isVisible) return;

        if (tutorialSoundBase != null)
        {
            tutorialSoundBase.PlaySound(TutorialSoundType.HidePanel);
        }

        KillAllAnimations();
        isVisible = false;

        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (scrollIndicator != null) scrollIndicator.SetActive(false);

        StopBlinkAnimation();

        currentSequence = DOTween.Sequence();
        currentSequence.Append(panelTransform.DOScale(Vector3.zero, scaleAnimDuration).SetEase(hideEase));
        currentSequence.Join(canvasGroup.DOFade(0f, fadeSpeed).SetEase(Ease.InQuad));
        currentSequence.OnComplete(() => tutorialPanel.SetActive(false));
        currentSequence.SetUpdate(true);
    }

    public void HideStepImmediate()
    {
        if (!isVisible) return;

        KillAllAnimations();
        isVisible = false;

        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (scrollIndicator != null) scrollIndicator.SetActive(false);

        StopBlinkAnimation();

        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (panelTransform != null) panelTransform.localScale = Vector3.zero;
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }

    private IEnumerator TypewriterEffect(string text)
    {
        yield return null;

        instructionText.text = "";
        instructionText.maxVisibleCharacters = 0;
        instructionText.text = text;

        if (textScrollRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(instructionText.rectTransform);
            if (instructionText.transform.parent != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(instructionText.transform.parent.GetComponent<RectTransform>());

            textScrollRect.verticalNormalizedPosition = 1f;
            UpdateScrollIndicator();
        }

        int totalCharacters = text.Length;
        keypressCounter = 0;

        for (int i = 0; i <= totalCharacters; i++)
        {
            instructionText.maxVisibleCharacters = i;
            if (i > 0 && i <= text.Length)
            {
                char addedChar = text[i - 1];
                if (!char.IsWhiteSpace(addedChar))
                {
                    keypressCounter++;
                    int k = Mathf.Max(1, keypressesPerSound);
                    if (writtingSoundBase != null && ((keypressCounter - 1) % k) == 0)
                    {
                        writtingSoundBase.PlayKeypressSound();
                    }
                }
            }
            yield return new WaitForSecondsRealtime(typewriterSpeed);
        }
        typewriterCoroutine = null;
    }

    private void OnScrollValueChanged(Vector2 position)
    {
        UpdateScrollIndicator();
    }

    private void UpdateScrollIndicator()
    {
        if (scrollIndicator == null || textScrollRect == null || indicatorCanvasGroup == null) return;

        bool atBottom = textScrollRect.verticalNormalizedPosition <= 0.01f;
        bool contentOverflows = textScrollRect.verticalScrollbar.size < 0.99f;

        bool showIndicator = contentOverflows && !atBottom;

        if (showIndicator)
        {
            scrollIndicator.SetActive(true);
            if (blinkSequence == null || !blinkSequence.IsActive())
            {
                StartBlinkAnimation();
            }
        }
        else
        {
            scrollIndicator.SetActive(false);
            StopBlinkAnimation();
        }
    }

    private void StartBlinkAnimation()
    {
        if (indicatorCanvasGroup == null) return;

        indicatorCanvasGroup.alpha = 1f;

        blinkSequence = DOTween.Sequence();
        blinkSequence.Append(indicatorCanvasGroup.DOFade(0.2f, blinkFadeTime).SetEase(Ease.InOutQuad))
                     .Append(indicatorCanvasGroup.DOFade(1f, blinkFadeTime).SetEase(Ease.InOutQuad))
                     .SetLoops(-1)
                     .SetUpdate(true);
    }

    private void StopBlinkAnimation()
    {
        if (blinkSequence != null && blinkSequence.IsActive())
        {
            blinkSequence.Kill();
        }
        blinkSequence = null;

        if (indicatorCanvasGroup != null)
        {
            indicatorCanvasGroup.alpha = 1f;
        }
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

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueButtonPressed);
        }

        if (textScrollRect != null)
        {
            textScrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        }
    }
}