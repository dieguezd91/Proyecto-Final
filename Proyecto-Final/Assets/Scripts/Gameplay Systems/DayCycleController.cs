using UnityEngine;
using UnityEngine.Events;

public class DayCycleController : MonoBehaviour
{
    public static DayCycleController Instance { get; private set; }

    [SerializeField] private int currentDay = 0;

    private int totalNights = 0;
    private bool weekResetPending = false;

    public int CurrentDay => currentDay;
    public int TotalNights => totalNights;

    public UnityEvent<int> OnNewDay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (OnNewDay == null)
        {
            OnNewDay = new UnityEvent<int>();
        }
    }

    public void StartDay()
    {
        GameFlowController.Instance.SetPhase(GamePhase.Day);
        OnNewDay?.Invoke(currentDay);
    }

    public void StartNight()
    {
        if (weekResetPending)
        {
            currentDay = 0;
            weekResetPending = false;
        }

        currentDay++;
        totalNights++;

        GameFlowController.Instance.SetPhase(GamePhase.Night);
    }

    public void RequestWeekReset()
    {
        weekResetPending = true;
    }

    public void ResetDayCount()
    {
        currentDay = 1;
    }
}