using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySoundBase : MonoBehaviour
{
    [Header("Sound Effects")]
    public SoundClipData steps;
    public SoundClipData spawning;
    public SoundClipData die;
    public SoundClipData hurt;
    public SoundClipData idle;
    public SoundClipData attack;
    
    [Header("Additional Sounds")]
    public SoundClipData special;
    public SoundClipData roar;
    public SoundClipData cast;
    
    // Dictionary for runtime access
    private Dictionary<EnemySoundType, SoundClipData> soundDictionary;
    
    public void Initialize()
    {
        soundDictionary = new Dictionary<EnemySoundType, SoundClipData>
        {
            { EnemySoundType.Steps, steps },
            { EnemySoundType.Spawning, spawning },
            { EnemySoundType.Die, die },
            { EnemySoundType.Hurt, hurt },
            { EnemySoundType.Idle, idle },
            { EnemySoundType.Attack, attack },
            { EnemySoundType.Special, special },
            { EnemySoundType.Cast, cast }
        };
    }
    
    public SoundClipData GetSound(EnemySoundType soundType)
    {
        if (soundDictionary == null) Initialize();
        
        soundDictionary.TryGetValue(soundType, out SoundClipData soundData);
        return soundData;
    }
    
    /// <summary>
    /// Plays the sound for the given EnemySoundType using SoundManager's pooling system.
    /// </summary>
    public void PlaySound(EnemySoundType soundType, SoundSourceType sourceType = SoundSourceType.Global, Transform parent = null)
    {
        var soundData = GetSound(soundType);
        if (soundData == null) return;
        if (!soundData.CanPlay()) return;
        SoundManager.Instance.PlayClip(soundData, sourceType, parent == null ? this.transform : parent);
        soundData.SetLastPlayTime();
    }
    
    
}
