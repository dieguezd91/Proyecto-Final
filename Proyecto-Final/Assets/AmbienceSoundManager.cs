using System.Collections;
using System;
using UnityEngine;

public enum AmbienceType { Forest, Infernum }

public class AmbienceSoundManager : MonoBehaviour
{
    [SerializeField] private SoundClipData _ambienceForest = new();
    [SerializeField] private SoundClipData _ambienceInfernum = new();

    [SerializeField] private AudioSource _audioSourceForest;
    [SerializeField] private AudioSource _audioSourceInfernum;

    [SerializeField] private float _crossfadeDuration = 1.5f;

    private Coroutine crossfadeCoroutine;
    
    public void StartForestAmbience()
    {
        if (!_audioSourceForest || !_ambienceForest.GetClip()) return;
        _audioSourceForest.clip = _ambienceForest.GetClip();
        _audioSourceForest.loop = true;
        _audioSourceForest.volume = 1f;
        _audioSourceForest.Play();
    }

    public void StopForestAmbience()
    {
        if (_audioSourceForest)
            _audioSourceForest.Stop();
    }

    public void StartInfernumAmbience()
    {
        if (!_audioSourceInfernum || !_ambienceInfernum.GetClip()) return;
        _audioSourceInfernum.clip = _ambienceInfernum.GetClip();
        _audioSourceInfernum.loop = true;
        _audioSourceInfernum.volume = 1f;
        _audioSourceInfernum.Play();
    }

    public void StopInfernumAmbience()
    {
        if (_audioSourceInfernum)
            _audioSourceInfernum.Stop();
    }

    public void TransitionAmbience(AmbienceType target, float duration = 1f)
    {
        if (crossfadeCoroutine != null)
            StopCoroutine(crossfadeCoroutine);
        crossfadeCoroutine = StartCoroutine(CrossfadeCoroutine(target, duration));
    }

    private IEnumerator CrossfadeCoroutine(AmbienceType target, float duration = 1f)
    {
        AudioSource fadeInSource = null;
        AudioSource fadeOutSource = null;
        SoundClipData fadeInClip = null;

        if (target == AmbienceType.Forest)
        {
            fadeInSource = _audioSourceForest;
            fadeOutSource = _audioSourceInfernum;
            fadeInClip = _ambienceForest;
        }
        else
        {
            fadeInSource = _audioSourceInfernum;
            fadeOutSource = _audioSourceForest;
            fadeInClip = _ambienceInfernum;
        }

        if (!fadeInSource.isPlaying)
        {
            fadeInSource.clip = fadeInClip.GetClip();
            fadeInSource.loop = true;
            fadeInSource.volume = 0f;
            fadeInSource.Play();
        }

        float startFadeIn = fadeInSource.volume;
        float startFadeOut = fadeOutSource.volume;
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            fadeInSource.volume = Mathf.Lerp(startFadeIn, 1f, t);
            fadeOutSource.volume = Mathf.Lerp(startFadeOut, 0f, t);
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        fadeInSource.volume = 1f;
        fadeOutSource.volume = 0f;
        if (fadeOutSource.isPlaying)
            fadeOutSource.Stop();
        crossfadeCoroutine = null;
    }
}