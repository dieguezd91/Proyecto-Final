using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeController : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Mute Button")]
    [SerializeField] private Button muteButton;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer; // Assign the 'Master Volume' mixer asset here
    // These must match the Exposed Parameter names in the 'Master Volume' mixer
    [SerializeField] private string masterVolumeParam = "Master"; // Exposed on 'Master' group
    [SerializeField] private string musicVolumeParam = "Music";   // Exposed on 'Music' subgroup
    [SerializeField] private string sfxVolumeParam = "Effects";       // Exposed on 'Effects' subgroup

    private float _savedMasterVolume;
    private float _savedMusicVolume;
    private float _savedSFXVolume;

    private bool isMuted = false;
    private float previousMasterVolume;
    private float previousMusicVolume;
    private float previousSFXVolume;

    private void Awake()
    {
        _savedMasterVolume = PlayerPrefs.GetFloat("gameVolume", 0.5f);
        _savedMusicVolume = PlayerPrefs.GetFloat("musicVolume", 0.5f);
        _savedSFXVolume = PlayerPrefs.GetFloat("sfxVolume", 0.5f);

        SetMixerVolume(masterVolumeParam, _savedMasterVolume);
        SetMixerVolume(musicVolumeParam, _savedMusicVolume);
        SetMixerVolume(sfxVolumeParam, _savedSFXVolume);
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
    }

    public void SetMasterVolume(float volume)
    {
        _savedMasterVolume = volume;
        SetMixerVolume(masterVolumeParam, volume);
        PlayerPrefs.SetFloat("gameVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        _savedMusicVolume = volume;
        SetMixerVolume(musicVolumeParam, volume);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        _savedSFXVolume = volume;
        SetMixerVolume(sfxVolumeParam, volume);
        PlayerPrefs.SetFloat("sfxVolume", volume);
    }

    private void SetMixerVolume(string parameter, float normalizedVolume)
    {
        // Convert [0,1] slider value to dB. -80 dB is silence, 0 dB is max.
        float dB = Mathf.Log10(Mathf.Clamp(normalizedVolume, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(parameter, dB);
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

            SetMixerVolume(masterVolumeParam, 0f);
            SetMixerVolume(musicVolumeParam, 0f);
            SetMixerVolume(sfxVolumeParam, 0f);

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
