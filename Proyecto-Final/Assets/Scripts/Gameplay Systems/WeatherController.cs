using System.Collections.Generic;
using UnityEngine;

public class WeatherController : MonoBehaviour
{
    [Header("Weather Settings")]
    [SerializeField] private List<ParticleSystem> dayWeatherPrefabs = new();
    [SerializeField] private List<AmbienceWeather> dayWeatherTypes = new(); // Map each prefab to a weather type

    [SerializeField] private Transform weatherParent;

    private ParticleSystem currentWeather;

    void Awake()
    {
        Debug.Log("WeatherController: Awake called");
        if (weatherParent == null) weatherParent = this.transform;
    }

    void Start()
    {
        Debug.Log("WeatherController: Start called");
        LevelManager.Instance.onNewDay.AddListener(HandleNewDay);
    }

    void OnDisable()
    {
        LevelManager.Instance.onNewDay.RemoveListener(HandleNewDay);
    }

    private void HandleNewDay(int dayIndex)
    {
        SpawnRandomWeather();
    }

    private void SpawnRandomWeather()
    {
        StopCurrentWeather();

        if (dayWeatherPrefabs == null || dayWeatherPrefabs.Count == 0) {
            Debug.LogWarning("WeatherController: No weather prefabs assigned.");
            return;
        }

        int idx = Random.Range(0, dayWeatherPrefabs.Count);

        currentWeather = Instantiate(dayWeatherPrefabs[idx], weatherParent);
        currentWeather.Play();

        // Set ambience weather type
        AmbienceWeather weatherType = AmbienceWeather.Clear;
        if (dayWeatherTypes != null && dayWeatherTypes.Count > idx)
            weatherType = dayWeatherTypes[idx];
        else
            Debug.LogWarning($"WeatherController: No weather type mapped for prefab index {idx}.");

        if (LevelManager.Instance.AmbienceSoundManager != null) {
            Debug.Log($"WeatherController: Calling TransitionAmbience with type Forest and weather {weatherType}.");
            LevelManager.Instance.AmbienceSoundManager.TransitionAmbience(AmbienceType.Forest, weatherType);
        } else {
            Debug.LogWarning("WeatherController: AmbienceSoundManager reference is null.");
        }
    }

    private void StopCurrentWeather()
    {
        if (currentWeather == null) return;

        currentWeather.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        Destroy(currentWeather.gameObject, 3f);
        currentWeather = null;
    }
}
