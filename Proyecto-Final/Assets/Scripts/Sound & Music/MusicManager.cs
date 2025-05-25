using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance; // Singleton instance

    [Header("Assign Audio Clips")]
    public AudioClip basicGameMusic;      
    public AudioClip secondGameMusic;     
    public AudioClip menuMusic; 

    private AudioSource audioSource;
    private string currentScene;
    private bool usingSecondGameMusic = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        UpdateMusicForScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMusicForScene();
    }

    private void UpdateMusicForScene()
    {
        currentScene = SceneManager.GetActiveScene().name;

        if (IsMenuScene())
        {
            PlayMusic(menuMusic);
        }
        else if (IsSecondGameScene())
        {
            PlayMusic(secondGameMusic);
            usingSecondGameMusic = true;
        }
        else
        {
            PlayMusic(usingSecondGameMusic ? secondGameMusic : basicGameMusic);
        }
    }

    private void PlayMusic(AudioClip clip)
    {
        if (audioSource.clip == clip) return; 

        audioSource.clip = clip;
        audioSource.Play();
    }

    private bool IsMenuScene()
    {
        return currentScene == "MenuScene" || currentScene == "Victoria" || currentScene == "Derrota" || currentScene == "LoadingScene";
    }

    private bool IsSecondGameScene()
    {
        return currentScene == "Level6" || currentScene == "Level7" || currentScene == "Level8" || currentScene == "Level9"; 
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

