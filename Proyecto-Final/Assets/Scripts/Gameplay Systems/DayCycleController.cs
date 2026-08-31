using UnityEngine;
using UnityEngine.Events;

public class DayCycleController : MonoBehaviour
{
    public static DayCycleController Instance { get; private set; }

    [SerializeField] private int currentDay = 0;

    public int CurrentDay => currentDay;

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
        currentDay++;
        GameFlowController.Instance.SetPhase(GamePhase.Night);
    }

    public void ResetDayCount()
    {
        currentDay = 1;
    }
}
