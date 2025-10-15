using System.Collections.Generic;
using UnityEngine;

public class PlantSoundBase : MonoBehaviour
{
    [Header("Sound Effects")]
    public SoundClipData planted;
    public SoundClipData removed;
    public SoundClipData harvested;
    public SoundClipData attack;
    public SoundClipData hurt;
    public SoundClipData die;

    // Dictionary for runtime access
    private Dictionary<PlantSoundType, SoundClipData> soundDictionary;

    public void Initialize()
    {
        soundDictionary = new Dictionary<PlantSoundType, SoundClipData>
        {
            { PlantSoundType.Planted, planted },
            { PlantSoundType.Removed, removed },
            { PlantSoundType.Harvested, harvested },
            { PlantSoundType.Attack, attack },
            { PlantSoundType.Hurt, hurt },
            { PlantSoundType.Die, die }
        };
    }

    public SoundClipData GetSound(PlantSoundType soundType)
    {
        if (soundDictionary == null) Initialize();
        soundDictionary.TryGetValue(soundType, out SoundClipData soundData);
        return soundData;
    }

    /// <summary>
    /// Plays the sound for the given PlantSoundType using SoundManager's pooling system.
    /// </summary>
    public void PlaySound(PlantSoundType soundType, SoundSourceType sourceType = SoundSourceType.Global, Transform parent = null)
    {
        var soundData = GetSound(soundType);
        if (soundData == null) return;
        if (!soundData.CanPlay()) return;
        SoundManager.Instance.PlayClip(soundData, sourceType, parent == null ? this.transform : parent);
        soundData.SetLastPlayTime();
    }
}
