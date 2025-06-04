using UnityEngine;
using UnityEngine.SceneManagement;


public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Clips de Audio")]
    [Tooltip("Música que se reproducirá en el menú")]
    public AudioClip menuMusic;

    [Tooltip("Música de fondo durante el día en la escena de juego")]
    public AudioClip dayMusic;

    [Tooltip("Música de fondo durante la noche en la escena de juego")]
    public AudioClip nightMusic;

    private AudioSource audioSource;

    private GameState lastKnownGameState;

    private void Awake()
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
        audioSource.playOnAwake = false;
        audioSource.volume = 0.5f;
        audioSource.mute = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        
        TrySubscribeToGameManager();

        PlayMusicAccordingToSceneOrState();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicAccordingToSceneOrState();
        TrySubscribeToGameManager();
    }

    
    private void Update()
    {
        if (GameManager.Instance != null)
        {
            GameState current = GameManager.Instance.GetCurrentGameState();
            if (current != lastKnownGameState)
            {
                HandleGameStateChanged(current);
                lastKnownGameState = current;
            }
        }
    }

    
    private void TrySubscribeToGameManager()
    {
        if (GameManager.Instance != null)
        {
            lastKnownGameState = GameManager.Instance.GetCurrentGameState();
        }
    }

    private void HandleGameStateChanged(GameState newState)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "MenuScene")
        {
            PlayMusic(menuMusic);
            return;
        }

        if (sceneName == "SampleScene")
        {
            if (newState == GameState.Day
                || newState == GameState.Digging
                || newState == GameState.Planting
                || newState == GameState.Harvesting
                || newState == GameState.Removing)
            {
                PlayMusic(dayMusic);
            }
            else if (newState == GameState.Night)
            {
                PlayMusic(nightMusic);
            }
            else
            {
                
                PlayMusic(dayMusic);
            }
        }
    }

    
    private void PlayMusicAccordingToSceneOrState()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "MenuScene")
        {
            PlayMusic(menuMusic);
            return;
        }

        if (sceneName == "SampleScene")
        {
            if (GameManager.Instance != null)
            {
                GameState current = GameManager.Instance.GetCurrentGameState();
                lastKnownGameState = current; 

                if (current == GameState.Day
                    || current == GameState.Digging
                    || current == GameState.Planting
                    || current == GameState.Harvesting
                    || current == GameState.Removing)
                {
                    PlayMusic(dayMusic);
                }
                else if (current == GameState.Night)
                {
                    PlayMusic(nightMusic);
                }
                else
                {
                    PlayMusic(dayMusic);
                }
                return;
            }
            else
            {
                PlayMusic(dayMusic);
                return;
            }
        }

      
    }

    
    private void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[MusicManager] AudioClip es null. Revisar asignación en el Inspector.");
            return;
        }

        if (audioSource.clip == clip && audioSource.isPlaying)
        {
           
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();
        Debug.Log($"[MusicManager] Reproduciendo: {clip.name}");
    }
}
