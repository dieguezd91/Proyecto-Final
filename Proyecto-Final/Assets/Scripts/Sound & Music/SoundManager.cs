using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public Sound[] sounds;

    [SerializeField] private List<AudioSource> audioSourcePool = new();
    [SerializeField] private int initialPoolSize = 5;

    private const int MaxSimultaneousSameSound = 3;

    // Dictionary to track sounds by name for quick lookup
    private Dictionary<string, Sound> soundLookup = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Create lookup dictionary for faster sound finding
        foreach (var sound in sounds)
        {
            soundLookup[sound.name] = sound;
        }

        // First, find any existing AudioSources on this GameObject
        AudioSource[] existingAudioSources = GetComponents<AudioSource>();
        audioSourcePool.AddRange(existingAudioSources);

        // If we don't have enough AudioSources, create more to reach initialPoolSize
        int sourcesToCreate = Mathf.Max(0, initialPoolSize - audioSourcePool.Count);
        for (int i = 0; i < sourcesToCreate; i++)
        {
            CreateNewAudioSource();
        }

        // Play sounds that should play on awake
        foreach (var sound in sounds)
        {
            if (sound.playOnAwake)
            {
                Play(sound.name);
            }
        }
    }

    private AudioSource CreateNewAudioSource()
    {
        var audioSource = gameObject.AddComponent<AudioSource>();
        audioSourcePool.Add(audioSource);
        return audioSource;
    }

    private AudioSource GetAvailableAudioSource()
    {
        // Try to find an available audio source (not playing)
        var availableSource = audioSourcePool.FirstOrDefault(source => !source.isPlaying);

        // If no available source, create a new one
        if (availableSource == null)
        {
            availableSource = CreateNewAudioSource();
        }

        return availableSource;
    }

    private void ConfigureAudioSource(AudioSource source, Sound sound)
    {
        source.clip = sound.clip;
        source.volume = sound.volume;
        source.pitch = sound.pitch;
        source.mute = sound.mute;
        source.loop = sound.loop;
    }

    public void PlayOneShot(string name)
    {
        if (!soundLookup.TryGetValue(name, out Sound sound))
        {
            Debug.LogWarning($"Sound '{name}' not found");
            return;
        }

        // Find all sources currently playing this sound
        var playingSources = audioSourcePool.Where(s => s.isPlaying && s.clip == sound.clip).ToList();
        if (playingSources.Count >= MaxSimultaneousSameSound)
        {
            // Restart the first one
            var sourceToRestart = playingSources[0];
            ConfigureAudioSource(sourceToRestart, sound);
            sourceToRestart.Stop();
            sourceToRestart.PlayOneShot(sound.clip, sound.volume);
            return;
        }

        var audioSource = GetAvailableAudioSource();
        ConfigureAudioSource(audioSource, sound);
        audioSource.PlayOneShot(sound.clip, sound.volume);
    }

    public void Play(string name)
    {
        if (!soundLookup.TryGetValue(name, out Sound sound))
        {
            Debug.LogWarning($"Sound '{name}' not found");
            return;
        }

        var playingSources = audioSourcePool.Where(s => s.isPlaying && s.clip == sound.clip).ToList();
        if (playingSources.Count >= MaxSimultaneousSameSound)
        {
            var sourceToRestart = playingSources[0];
            ConfigureAudioSource(sourceToRestart, sound);
            sourceToRestart.Stop();
            sourceToRestart.Play();
            return;
        }

        var audioSource = GetAvailableAudioSource();
        ConfigureAudioSource(audioSource, sound);
        audioSource.Play();
    }

    public void PlayLoop(string name)
    {
        if (!soundLookup.TryGetValue(name, out Sound sound))
        {
            Debug.LogWarning($"Sound '{name}' not found");
            return;
        }

        var playingSources = audioSourcePool.Where(s => s.isPlaying && s.clip == sound.clip && s.loop).ToList();
        if (playingSources.Count >= MaxSimultaneousSameSound)
        {
            var sourceToRestart = playingSources[0];
            ConfigureAudioSource(sourceToRestart, sound);
            sourceToRestart.Stop();
            sourceToRestart.loop = true;
            sourceToRestart.Play();
            return;
        }

        // Check if this sound is already playing and looping
        var existingSource = audioSourcePool.FirstOrDefault(source =>
            source.isPlaying && source.clip == sound.clip && source.loop);
        if (existingSource != null)
        {
            return; // Already playing this looped sound
        }

        var audioSource = GetAvailableAudioSource();
        ConfigureAudioSource(audioSource, sound);
        audioSource.loop = true;
        audioSource.Play();
    }

    public void Stop(string name)
    {
        if (!soundLookup.TryGetValue(name, out Sound sound))
        {
            Debug.LogWarning($"Sound '{name}' not found");
            return;
        }

        // Stop all audio sources playing this sound
        foreach (var source in audioSourcePool)
        {
            if (source.isPlaying && source.clip == sound.clip)
            {
                source.Stop();
            }
        }
    }

    public void StopAll()
    {
        foreach (var source in audioSourcePool)
        {
            if (source.isPlaying)
                source.Stop();
        }
    }

    public void PauseAll()
    {
        foreach (var source in audioSourcePool)
        {
            if (source.isPlaying)
                source.Pause();
        }
    }

    public void ResumeAll()
    {
        foreach (var source in audioSourcePool)
        {
            if (source.clip != null && !source.isPlaying)
                source.UnPause();
        }
    }
    
    public bool IsPlaying(string name)
    {
        if (!soundLookup.TryGetValue(name, out Sound sound))
            return false;

        return audioSourcePool.Any(source => source.isPlaying && source.clip == sound.clip);
    }

    public int GetActiveSourceCount()
    {
        return audioSourcePool.Count(source => source.isPlaying);
    }

    public int GetTotalSourceCount()
    {
        return audioSourcePool.Count;
    }

    /// <summary>
    /// Plays a SoundClipData using the pooling and limiting system.
    /// </summary>
    public void PlayClip(SoundClipData data)
    {
        if (data == null) return;
        var clip = data.GetClip();
        if (clip == null) return;

        // Find all sources currently playing this clip
        var playingSources = audioSourcePool.Where(s => s.isPlaying && s.clip == clip).ToList();
        if (playingSources.Count >= MaxSimultaneousSameSound)
        {
            // Restart the first one
            var sourceToRestart = playingSources[0];
            sourceToRestart.Stop();
            sourceToRestart.clip = clip;
            sourceToRestart.volume = data.volume;
            sourceToRestart.pitch = data.GetPitch();
            sourceToRestart.loop = data.loop;
            sourceToRestart.mute = false;
            if (data.loop)
                sourceToRestart.Play();
            else
                sourceToRestart.PlayOneShot(clip, data.volume);
            return;
        }

        var audioSource = GetAvailableAudioSource();
        audioSource.clip = clip;
        audioSource.volume = data.volume;
        audioSource.pitch = data.GetPitch();
        audioSource.loop = data.loop;
        audioSource.mute = false;
        if (data.loop)
            audioSource.Play();
        else
            audioSource.PlayOneShot(clip, data.volume);
    }
}