using System.Collections.Generic;
using UnityEngine;

public class WeatherController : MonoBehaviour
{
    [Header("Weather Settings")]
    [SerializeField] private List<ParticleSystem> dayWeatherPrefabs = new();

    [SerializeField] private Transform weatherParent;

    private ParticleSystem currentWeather;

    void Awake()
    {
        if (weatherParent == null) weatherParent = this.transform;
    }

    void Start()
    {
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

        if (dayWeatherPrefabs == null || dayWeatherPrefabs.Count == 0) return;

        int idx = Random.Range(0, dayWeatherPrefabs.Count);

        currentWeather = Instantiate(dayWeatherPrefabs[idx], weatherParent);
        currentWeather.Play();
    }

    private void StopCurrentWeather()
    {
        if (currentWeather == null) return;

        currentWeather.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        Destroy(currentWeather.gameObject, 3f);
        currentWeather = null;
    }
}
