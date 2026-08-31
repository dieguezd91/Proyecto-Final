using TMPro;
using UnityEngine;

public class DayTimerController : MonoBehaviour
{
    [SerializeField] private PauseController pauseController;
    [Header("Timer Settings")]
    [SerializeField] private float dayDuration = 180f;
    [SerializeField] private TMP_Text timerText;

    [Header("UI")]
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private Animator timerAnimator;
    [SerializeField] private string visibleBoolName = "IsVisible";

    private float remainingTime;
    private bool countdownActive = true;
    private bool resetWhenDayStarts = true;
    public bool NightStartedByTimer { get; private set; }

    private void Awake()
    {
        if (pauseController == null) pauseController = FindObjectOfType<PauseController>();
        remainingTime = dayDuration;
        UpdateTimerText();
    }

    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            GameFlowController.Instance.OnPhaseChanged += HandleGameStateChanged;
            HandleGameStateChanged(GameFlowController.Instance.CurrentPhase);
        }
        
    }
    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        if (LevelManager.Instance != null)
            GameFlowController.Instance.OnPhaseChanged -= HandleGameStateChanged;
    }

    private void Update()
    {
        if (!countdownActive)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            UpdateTimerText();

            countdownActive = false;
            resetWhenDayStarts = true;
            NightStartedByTimer = true;
            LevelManager.Instance?.TransitionToNight();

            return;
        }

        UpdateTimerText();
    }

    private void HandleGameStateChanged(GamePhase newPhase)
    {

        
        bool isDayState = IsDayGameplayPhase(newPhase);
        
        SetVisible(isDayState);

        if (newPhase == GamePhase.Night)
        {
            countdownActive = false;
            resetWhenDayStarts = true;
            return;
        }

        if (isDayState)
        {
            if (resetWhenDayStarts)
            {
                ResetTimer();
                resetWhenDayStarts = false;
                NightStartedByTimer = false;
            }

            countdownActive = true;
        }
        else
        {
            countdownActive = false;
        }
    }

    private bool IsDayGameplayPhase(GamePhase phase)
    {
        return phase == GamePhase.Day ||
               phase == GamePhase.None ||
               (pauseController != null && pauseController.IsPaused);
    }

    private void SetVisible(bool visible)
    {
        if (timerAnimator != null)
        {
            timerAnimator.SetBool(visibleBoolName, visible);
        }
        else if (timerPanel != null)
        {
            timerPanel.SetActive(visible);
        }
    }

    private void ResetTimer()
    {
        remainingTime = dayDuration;
        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
