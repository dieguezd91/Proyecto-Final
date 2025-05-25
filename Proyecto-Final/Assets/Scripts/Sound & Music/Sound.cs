using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0, 1)]
    public float volume = 1;
    public float pitch = 1;
    public bool playOnAwake;
    public bool loop;
    public bool mute;


    public AudioSource audioSource;


}