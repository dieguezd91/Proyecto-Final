using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AmbienceWeatherAudioSource : MonoBehaviour
{
    [Header("Ambience Settings")]
    public AmbienceType AmbienceType;
    public AmbienceWeather AmbienceWeather;
    [SerializeField] private AudioSource _audioSource;

    private void Reset()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void Play()
    {
        if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
        _audioSource.Play();
    }

    public void StopClip()
    {
        if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
        if (_audioSource.isPlaying) _audioSource.Stop();
    }

    public bool IsPlaying => _audioSource != null && _audioSource.isPlaying;
    
    public AudioClip GetAudioClip => _audioSource.clip;
}

