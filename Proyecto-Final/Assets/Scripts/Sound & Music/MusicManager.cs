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

    
    private GameState lastOfficialGameState = GameState.None;

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
        audioSource.volume = 0.4f;
        audioSource.mute = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        PlayMusicAccordingToSceneOrState();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Se dispara cada vez que se carga una nueva escena.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicAccordingToSceneOrState();
    }

    private void Update()
    {
        if (GameManager.Instance == null)
            return;

        GameState currentState = GameManager.Instance.GetCurrentGameState();
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "MenuScene")
            return;

        if (sceneName == "SampleScene" || sceneName == "GameScene")
        {
            if (IsDaylikeState(currentState))
            {
                if (lastOfficialGameState == GameState.Day)
                    return;

                lastOfficialGameState = GameState.Day;
                PlayMusic(dayMusic);
            }
            else if (currentState == GameState.Night)
            {
                if (lastOfficialGameState == GameState.Night)
                    return;

                lastOfficialGameState = GameState.Night;
                PlayMusic(nightMusic);
            }
        }
    }

    /// <summary>
    /// Reproduce la música correcta al cargar una escena o al iniciar el juego.
    /// </summary>
    private void PlayMusicAccordingToSceneOrState()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "MenuScene")
        {
            lastOfficialGameState = GameState.None;
            PlayMusic(menuMusic);
            return;
        }

        if (sceneName == "SampleScene" || sceneName == "GameScene")
        {
            if (GameManager.Instance != null)
            {
                GameState current = GameManager.Instance.GetCurrentGameState();

                if (IsDaylikeState(current))
                {
                    lastOfficialGameState = GameState.Day;
                    PlayMusic(dayMusic);
                }
                else if (current == GameState.Night)
                {
                    lastOfficialGameState = GameState.Night;
                    PlayMusic(nightMusic);
                }
                else
                {
                    lastOfficialGameState = GameState.Day;
                    PlayMusic(dayMusic);
                }
            }
            else
            {
                lastOfficialGameState = GameState.Day;
                PlayMusic(dayMusic);
            }
        }

    }

    /// <summary>
    /// Método auxiliar que devuelve true si el estado dado debe contarse como “día” a efectos de música.
    /// </summary>
    private bool IsDaylikeState(GameState state)
    {
        return state == GameState.Day
               || state == GameState.Digging
               || state == GameState.Planting
               || state == GameState.Harvesting
               || state == GameState.Removing;
    }

    /// <summary>
    /// Asigna el clip al AudioSource y lo reproduce. Si ya era el mismo clip, no hace nada.
    /// </summary>
    private void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[MusicManager] AudioClip es null. Revisa la asignación en el Inspector.");
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
