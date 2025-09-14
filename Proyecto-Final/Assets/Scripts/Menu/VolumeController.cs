using UnityEngine;
using UnityEngine.UI;

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

    private void Awake()
    {
        var settings = GameManager.Instance.SettingsManager;
        _savedMasterVolume = settings.GetMasterVolume();
        _savedMusicVolume = settings.GetMusicVolume();
        _savedSFXVolume = settings.GetSFXVolume();
        Application.quitting += OnApplicationQuit;
    }

    private void Start()
    {
        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(_savedMasterVolume);
            volumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(_savedMusicVolume);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(_savedSFXVolume);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (muteButton != null)
            muteButton.onClick.AddListener(ToggleMute);
    }

    public void SetMasterVolume(float volume)
    {
        _savedMasterVolume = volume;
        GameManager.Instance.SettingsManager.SetMasterVolume(volume);
    }

    public void SetMusicVolume(float volume)
    {
        _savedMusicVolume = volume;
        GameManager.Instance.SettingsManager.SetMusicVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        _savedSFXVolume = volume;
        GameManager.Instance.SettingsManager.SetSFXVolume(volume);
    }

    private void OnApplicationQuit()
    {
        GameManager.Instance.SettingsManager.Save();
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
