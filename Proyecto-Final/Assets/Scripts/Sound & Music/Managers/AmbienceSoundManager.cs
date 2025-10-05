using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AmbienceSoundManager : MonoBehaviour
{
    [Header("Weather Audio Sources")]
    [SerializeField] private List<AmbienceWeatherAudioSource> _weatherAudioSources = new();
    private AmbienceWeatherAudioSource currentPlayingSource;

    private void Awake() {
        // Find all AmbienceWeatherAudioSource components in children if not assigned
        if (_weatherAudioSources == null || _weatherAudioSources.Count == 0)
            _weatherAudioSources = GetComponentsInChildren<AmbienceWeatherAudioSource>().ToList();
    }

    private AmbienceWeatherAudioSource GetWeatherSource(AmbienceType type, AmbienceWeather weather) {
        return _weatherAudioSources.FirstOrDefault(s => s.AmbienceType == type && s.AmbienceWeather == weather);
    }

    private void StartAmbience(AmbienceType type, AmbienceWeather weather) {
        var source = GetWeatherSource(type, weather);
        
        if (source == null) {
            return;
        }
        
        if (currentPlayingSource != null && currentPlayingSource != source)
            currentPlayingSource.StopClip();
        
        source.Play();
        currentPlayingSource = source;
    }

    public void TransitionAmbience(AmbienceType target, AmbienceWeather weather) {
        StartAmbience(target, weather);
    }
}