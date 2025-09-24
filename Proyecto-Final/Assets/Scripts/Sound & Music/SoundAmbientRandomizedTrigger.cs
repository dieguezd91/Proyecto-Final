using UnityEngine;

/// <summary>
/// This script will be used to play ambient sounds, if the player is inside the trigger area for it.
/// It will play at random intervals, but the timer stops if player is not inside trigger area.
/// The audio source for this is going to be 3D for immersion.
/// </summary>

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class SoundAmbientRandomizedTrigger : MonoBehaviour
{
    [SerializeField] private float _minTime = 5f;
    [SerializeField] private float _maxTime = 15f;
    [SerializeField] private SoundClipData _ambientClip;
    [SerializeField] private string _playerTag = "Player";

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private float _timer;
    [SerializeField] private bool _playerInside;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.spatialBlend = 1f; // 3D sound
        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
        _audioSource.clip = _ambientClip.GetClip();
        ResetTimer();
    }

    private void Update()
    {
        if (!_playerInside) return;
        _timer -= Time.deltaTime;
        if (!(_timer <= 0f)) return;
        PlayAmbientSound();
        ResetTimer();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(_playerTag))
        {
            _playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(_playerTag))
        {
            _playerInside = false;
        }
    }

    private void PlayAmbientSound()
    {
        var clip = _ambientClip.GetClip();
        if (_audioSource && clip)
            _audioSource.PlayOneShot(clip);
    }

    private void ResetTimer()
    {
        _timer = Random.Range(_minTime, _maxTime);
    }
}