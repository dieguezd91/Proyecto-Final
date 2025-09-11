using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    private static PlantManager _instance;
    public static PlantManager Instance { get { return _instance; } }

    private Dictionary<Vector3Int, Plant> tilePlants;

    [Header("SETTINGS")]
    [SerializeField] private bool notifyPlantsOnNewDay = false;

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
        lastGameState = LevelManager.Instance.currentGameState;

        if (LevelManager.Instance != null && notifyPlantsOnNewDay)
            LevelManager.Instance.onNewDay.AddListener(OnGameManagerNewDay);
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.onNewDay.RemoveListener(OnGameManagerNewDay);
        }
    }

    private void OnGameManagerNewDay(int currentDay)
    {
        dayCounter = currentDay;
        if (notifyPlantsOnNewDay)
        {
            NotifyAllPlantsNewDay();
        }
    }

    public void RegisterPlant(Plant plant)
    {
        if (plant != null && !registeredPlants.Contains(plant))
        {
            registeredPlants.Add(plant);
        }
    }

    public void UnregisterPlant(Plant plant)
    {
        if (plant != null && registeredPlants.Contains(plant))
        {
            registeredPlants.Remove(plant);
        }
    }

    private void NotifyAllPlantsNewDay()
    {
        registeredPlants.RemoveAll(p => p == null);

        foreach (Plant plant in registeredPlants)
        {
            if (plant != null && plant.plantingDay > 0)
            {
                plant.SendMessage("OnNewDay", dayCounter, SendMessageOptions.DontRequireReceiver);
            }
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

    public List<ResourcePlant> GetHarvestablePlants()
    {
        List<ResourcePlant> harvestablePlants = new List<ResourcePlant>();

        foreach (Plant plant in registeredPlants)
        {
            if (plant is ResourcePlant resourcePlant && resourcePlant.IsReadyToHarvest())
            {
                harvestablePlants.Add(resourcePlant);
            }
        }

        return harvestablePlants;
    }

    public List<T> GetPlantsByType<T>() where T : Plant
    {
        List<T> result = new List<T>();

        foreach (Plant plant in registeredPlants)
        {
            if (plant is T typedPlant)
            {
                result.Add(typedPlant);
            }
        }

        return result;
    }
}