using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private string _masterVolumeParam = "Master";
    [SerializeField] private string _musicVolumeParam = "Music";
    [SerializeField] private string _sfxVolumeParam = "Effects";
    [SerializeField] private float _masterVolume = 0.5f;
    [SerializeField] private float _musicVolume = 0.5f;
    [SerializeField] private float _sfxVolume = 0.5f;

    public float GetMasterVolume() => _masterVolume;
    public float GetMusicVolume() => _musicVolume;
    public float GetSFXVolume() => _sfxVolume;

    private void Awake()
    {
        Debug.Log("[SettingsManager] Awake called");
        Load();
    }

    private void Start()
    {
        Debug.Log("[SettingsManager] Start called - ensuring mixer values are set after audio system is ready");
        SetMixerVolume(_masterVolumeParam, _masterVolume);
        SetMixerVolume(_musicVolumeParam, _musicVolume);
        SetMixerVolume(_sfxVolumeParam, _sfxVolume);
    }

    private void SetMixerVolume(string parameter, float normalizedVolume)
    {
        if (_audioMixer == null)
        {
            Debug.LogWarning($"[SettingsManager] AudioMixer is null when trying to set {parameter} to {normalizedVolume}");
            return;
        }
        float dB = Mathf.Log10(Mathf.Clamp(normalizedVolume, 0.0001f, 1f)) * 20f;
        Debug.Log($"[SettingsManager] Setting mixer parameter '{parameter}' to {dB} dB (normalized: {normalizedVolume})");
        _audioMixer.SetFloat(parameter, dB);
    }

    public void SetMasterVolume(float value)
    {
        Debug.Log($"[SettingsManager] SetMasterVolume called with value: {value}");
        _masterVolume = value;
        PlayerPrefs.SetFloat(_masterVolumeParam, value);
        SetMixerVolume(_masterVolumeParam, value);
    }

    public void SetMusicVolume(float value)
    {
        Debug.Log($"[SettingsManager] SetMusicVolume called with value: {value}");
        _musicVolume = value;
        PlayerPrefs.SetFloat(_musicVolumeParam, value);
        SetMixerVolume(_musicVolumeParam, value);
    }

    public void SetSFXVolume(float value)
    {
        Debug.Log($"[SettingsManager] SetSFXVolume called with value: {value}");
        _sfxVolume = value;
        PlayerPrefs.SetFloat(_sfxVolumeParam, value);
        SetMixerVolume(_sfxVolumeParam, value);
    }

    public void Save() => PlayerPrefs.Save();

    public void Load()
    {
        Debug.Log("[SettingsManager] Load called");
        _masterVolume = PlayerPrefs.GetFloat(_masterVolumeParam, 0.5f);
        _musicVolume = PlayerPrefs.GetFloat(_musicVolumeParam, 0.5f);
        _sfxVolume = PlayerPrefs.GetFloat(_sfxVolumeParam, 0.5f);
        Debug.Log($"[SettingsManager] Loaded values: Master={_masterVolume}, Music={_musicVolume}, SFX={_sfxVolume}");
        SetMasterVolume(_masterVolume);
        SetMusicVolume(_musicVolume);
        SetSFXVolume(_sfxVolume);
    }
}
