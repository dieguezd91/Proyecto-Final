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
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    private void Start()
    {
        foreach (var sound in sounds)
        {
            sound.audioSource = this.gameObject.AddComponent<AudioSource>();
            sound.audioSource.volume = sound.volume;
            sound.audioSource.pitch = sound.pitch;
            sound.audioSource.playOnAwake = sound.playOnAwake;
            sound.audioSource.loop = sound.loop;
            sound.audioSource.mute = sound.mute;

            if (sound.playOnAwake)
            {
                sound.audioSource.Play();
            }
        }
    }



    public void PlaySound(string name)
    {
        Sound sound = Array.Find(sounds, x => x.name == name);
        if (sound != null)
        {
            sound.audioSource.PlayOneShot(sound.clip);
        }
        else
        {
            Debug.LogError("Sound doesn't exist");
        }
    }
}
