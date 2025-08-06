using System.Collections.Generic;
using UnityEngine;
using System;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public Sound[] sounds;

    private List<AudioSource> allAudioSources = new List<AudioSource>();

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
        foreach (var sound in sounds)
        {
            var src = gameObject.AddComponent<AudioSource>();
            sound.audioSource = src;

            src.clip = sound.clip;
            src.pitch = sound.pitch;
            src.playOnAwake = sound.playOnAwake;
            src.loop = sound.loop;
            src.mute = sound.mute;
            src.volume = sound.volume;

            allAudioSources.Add(src);

            if (sound.playOnAwake)
                src.Play();
        }
    }

    public void PlayOneShot(string name)
    {
        var s = Array.Find(sounds, x => x.name == name);
        if (s != null)
            s.audioSource.PlayOneShot(s.clip);
        else
            Debug.LogWarning($"Sound '{name}' not found");
    }

    public void Play(string name)
    {
        var s = Array.Find(sounds, x => x.name == name);
        if (s != null)
            s.audioSource.Play();
        else if (s == null)
            Debug.LogWarning($"Sound '{name}' not found");
    }

    public void PlayLoop(string name)
    {
        var s = Array.Find(sounds, x => x.name == name);
        if (s != null)
        {
            if (!s.audioSource.isPlaying)
            {
                s.audioSource.loop = true;
                s.audioSource.Play();
            }
        }
        else
        {
            Debug.LogWarning($"Sound '{name}' not found");
        }
    }

    public void Stop(string name)
    {
        var s = Array.Find(sounds, x => x.name == name);
        if (s != null && s.audioSource.isPlaying)
            s.audioSource.Stop();
    }

    public void PauseAll()
    {
        foreach (var source in allAudioSources)
        {
            if (source.isPlaying)
                source.Pause();
        }
    }

    public void ResumeAll()
    {
        foreach (var source in allAudioSources)
        {
            if (source.clip != null && !source.isPlaying)
                source.UnPause();
        }
    }
}
