using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    private float _savedVolume;

    private void Awake()
    {
        _savedVolume = PlayerPrefs.GetFloat("gameVolume", 0.5f);
        AudioListener.volume = _savedVolume;

        Application.quitting += OnApplicationQuit;
    }

    private void Start()
    {
        if (volumeSlider != null)
            volumeSlider.value = _savedVolume;

        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        _savedVolume = volume;
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("gameVolume", volume);
        PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        AudioListener.volume = _savedVolume;
    }

    private void OnDestroy()
    {
        volumeSlider.onValueChanged.RemoveListener(SetVolume);
        Application.quitting -= OnApplicationQuit;
    }
}
