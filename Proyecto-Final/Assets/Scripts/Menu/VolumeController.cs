using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class VolumeController : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Mute Button")]
    [SerializeField] private Button muteButton;


    private float _savedMasterVolume;
    private float _savedMusicVolume;
    private float _savedSFXVolume;

    private bool isMuted = false;
    private float previousMasterVolume;
    private float previousMusicVolume;
    private float previousSFXVolume;

    private AudioSource musicSource;
    private readonly HashSet<AudioSource> trackedSFXSources = new();

    private float scanTimer = 0f;
    private const float scanInterval = 1f;

    private void Awake()
    {
        _savedMasterVolume = PlayerPrefs.GetFloat("gameVolume", 0.5f);
        _savedMusicVolume = PlayerPrefs.GetFloat("musicVolume", 0.5f);
        _savedSFXVolume = PlayerPrefs.GetFloat("sfxVolume", 0.5f);

        AudioListener.volume = _savedMasterVolume;
        Application.quitting += OnApplicationQuit;
    }

    private void Start()
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = _savedMasterVolume;
            volumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (musicSlider != null)
        {
            musicSlider.value = _savedMusicVolume;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = _savedSFXVolume;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (muteButton != null)
            muteButton.onClick.AddListener(ToggleMute);

        ScanAudioSources();
    }

    private void Update()
    {
        scanTimer += Time.unscaledDeltaTime;
        if (scanTimer >= scanInterval)
        {
            ScanAudioSources();
            scanTimer = 0f;
        }
    }

    private void ScanAudioSources()
    {
        AudioSource[] allSources = FindObjectsOfType<AudioSource>();

        foreach (AudioSource source in allSources)
        {
            if (source == null)
                continue;

            if (musicSource == null && source.loop && source.isPlaying)
            {
                musicSource = source;
                musicSource.volume = _savedMusicVolume;
                continue;
            }

            if (!trackedSFXSources.Contains(source) && source != musicSource)
            {
                source.volume = _savedSFXVolume;
                trackedSFXSources.Add(source);
            }
        }
    }

    public void SetMasterVolume(float volume)
    {
        _savedMasterVolume = volume;
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("gameVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        _savedMusicVolume = volume;

        if (musicSource != null)
            musicSource.volume = volume;

        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        _savedSFXVolume = volume;

        foreach (var sfx in trackedSFXSources)
        {
            if (sfx != null && sfx != musicSource)
                sfx.volume = volume;
        }

        PlayerPrefs.SetFloat("sfxVolume", volume);
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    private void ToggleMute()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            previousMasterVolume = _savedMasterVolume;
            previousMusicVolume = _savedMusicVolume;
            previousSFXVolume = _savedSFXVolume;

            SetMasterVolume(0f);
            SetMusicVolume(0f);
            SetSFXVolume(0f);

            if (volumeSlider != null) volumeSlider.value = 0f;
            if (musicSlider != null) musicSlider.value = 0f;
            if (sfxSlider != null) sfxSlider.value = 0f;
        }
        else
        {
            SetMasterVolume(previousMasterVolume);
            SetMusicVolume(previousMusicVolume);
            SetSFXVolume(previousSFXVolume);

            if (volumeSlider != null) volumeSlider.value = previousMasterVolume;
            if (musicSlider != null) musicSlider.value = previousMusicVolume;
            if (sfxSlider != null) sfxSlider.value = previousSFXVolume;
        }
    }


    private void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);

        if (muteButton != null)
            muteButton.onClick.RemoveListener(ToggleMute);


        Application.quitting -= OnApplicationQuit;
    }
}
