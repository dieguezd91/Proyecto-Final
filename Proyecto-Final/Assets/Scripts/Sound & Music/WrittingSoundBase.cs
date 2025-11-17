using System.Collections.Generic;
using UnityEngine;

public class WrittingSoundBase : MonoBehaviour
{
    [Header("Keypress Sounds")]
    public List<SoundClipData> keypressSounds;

    private int lastPlayedIndex = -1;

    public void PlayKeypressSound()
    {
        if (keypressSounds == null || keypressSounds.Count == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] WrittingSoundBase: No keypress sounds assigned.");
            return;
        }
        var clipCount = keypressSounds.Count;
        var tempList = keypressSounds;
        if (clipCount > 1 && lastPlayedIndex >= 0 && lastPlayedIndex < clipCount)
        {
            tempList = new List<SoundClipData>(keypressSounds);
            tempList.RemoveAt(lastPlayedIndex);
        }
        var index = Random.Range(0, tempList.Count);
        var soundData = tempList[index];
        if (soundData == null || soundData.GetClip() == null)
        {
            Debug.LogWarning($"[{gameObject.name}] WrittingSoundBase: Keypress SoundClipData or AudioClip is missing.");
            return;
        }
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning($"[{gameObject.name}] WrittingSoundBase: SoundManager.Instance is null. Cannot play keypress sound.");
            return;
        }
        SoundManager.Instance.PlayClip(soundData, SoundSourceType.Global, Camera.main?.transform);
        // Update lastPlayedIndex to the index in the original list
        lastPlayedIndex = keypressSounds.IndexOf(soundData);
        Debug.Log($"[{gameObject.name}] WrittingSoundBase: Played keypress sound '{soundData.GetClip().name}' (index {lastPlayedIndex}).");
    }
}
