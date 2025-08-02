using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public Sound[] sounds;

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

            if (sound.playOnAwake)
                src.Play();
        }
    }

    /// <summary>Reproduce el clip una sola vez.</summary>
    public void PlayOneShot(string name)
    {
        var s = Array.Find(sounds, x => x.name == name);
        if (s != null)
            s.audioSource.PlayOneShot(s.clip);
        else
            Debug.LogWarning($"Sound '{name}' not found");
    }

    /// <summary>Arranca la fuente en loop (si no lo está ya).</summary>
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

    /// <summary>Detiene la fuente si está sonando.</summary>
    public void Stop(string name)
    {
        var s = Array.Find(sounds, x => x.name == name);
        if (s != null && s.audioSource.isPlaying)
            s.audioSource.Stop();
    }
}
