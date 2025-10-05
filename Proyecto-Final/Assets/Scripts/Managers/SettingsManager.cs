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
        Load();
    }

    private void Start()
    {
        SetMixerVolume(_masterVolumeParam, _masterVolume);
        SetMixerVolume(_musicVolumeParam, _musicVolume);
        SetMixerVolume(_sfxVolumeParam, _sfxVolume);
    }

    private void SetMixerVolume(string parameter, float normalizedVolume)
    {
        if (_audioMixer == null)
        {
            return;
        }
        float dB = Mathf.Log10(Mathf.Clamp(normalizedVolume, 0.0001f, 1f)) * 20f;
        _audioMixer.SetFloat(parameter, dB);
    }

    public void SetMasterVolume(float value)
    {
        _masterVolume = value;
        PlayerPrefs.SetFloat(_masterVolumeParam, value);
        SetMixerVolume(_masterVolumeParam, value);
    }

    public void SetMusicVolume(float value)
    {
        _musicVolume = value;
        PlayerPrefs.SetFloat(_musicVolumeParam, value);
        SetMixerVolume(_musicVolumeParam, value);
    }

    public void SetSFXVolume(float value)
    {
        _sfxVolume = value;
        PlayerPrefs.SetFloat(_sfxVolumeParam, value);
        SetMixerVolume(_sfxVolumeParam, value);
    }

    public void Save() => PlayerPrefs.Save();

    public void Load()
    {
        _masterVolume = PlayerPrefs.GetFloat(_masterVolumeParam, 0.5f);
        _musicVolume = PlayerPrefs.GetFloat(_musicVolumeParam, 0.5f);
        _sfxVolume = PlayerPrefs.GetFloat(_sfxVolumeParam, 0.5f);
        SetMasterVolume(_masterVolume);
        SetMusicVolume(_musicVolume);
        SetSFXVolume(_sfxVolume);
    }
}
