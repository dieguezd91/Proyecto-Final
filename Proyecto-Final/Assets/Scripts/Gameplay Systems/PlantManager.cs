using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    private static PlantManager _instance;
    public static PlantManager Instance { get { return _instance; } }

    [Header("Configuration")]
    [SerializeField] private bool notifyPlantsOnNewDay = true;

    private List<Plant> registeredPlants = new List<Plant>();
    private GameState lastGameState = GameState.None;
    private int dayCounter = 0;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    private void Start()
    {
        lastGameState = GameManager.Instance.currentGameState;
    }

    private void Update()
    {
        GameState currentState = GameManager.Instance.currentGameState;

        if (lastGameState == GameState.Night && currentState == GameState.Day)
        {
            dayCounter++;

            if (notifyPlantsOnNewDay)
            {
                NotifyAllPlantsNewDay();
            }
        }

        lastGameState = currentState;
    }

    public void RegisterPlant(Plant plant)
    {
        if (!registeredPlants.Contains(plant))
        {
            registeredPlants.Add(plant);
        }
    }

    public void UnregisterPlant(Plant plant)
    {
        if (registeredPlants.Contains(plant))
        {
            registeredPlants.Remove(plant);
        }
    }

    private void NotifyAllPlantsNewDay()
    {
        registeredPlants.RemoveAll(p => p == null);

        foreach (Plant plant in registeredPlants)
        {
            plant.SendMessage("OnNewDay", dayCounter, SendMessageOptions.DontRequireReceiver);
        }
    }

    public int GetDayCounter()
    {
        return dayCounter;
    }

    public void ResetDayCounter()
    {
        dayCounter = 0;
    }
}