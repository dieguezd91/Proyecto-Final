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

    // Último GameState “oficial” (solo Day o Night) que reprodujo música.
    // Lo inicializamos en un valor inválido para forzar una primera reproducción.
    private GameState lastOfficialGameState = GameState.None;

    private void Awake()
    {
        // Singleton básico
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Creamos AudioSource en tiempo de ejecución
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0.5f;
        audioSource.mute = false;

        // Escuchar carga de escenas
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Al arrancar, reproducir según la escena actual / estado
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
        // Solo cruzamos de Day <-> Night; ignoramos Paused, OnInventory, etc.
        if (GameManager.Instance == null)
            return;

        GameState currentState = GameManager.Instance.GetCurrentGameState();
        string sceneName = SceneManager.GetActiveScene().name;

        // Si estamos en el menú, siempre mantener menuMusic y no hacer polling
        if (sceneName == "MenuScene")
            return;

        // Si estamos en la escena de juego y el estado real es “Day” o “Night”:
        if (sceneName == "SampleScene" || sceneName == "GameScene" /* ajusta tu nombre real */)
        {
            // Si el estado actual es “Day” (o cualquiera que consideres día activa)
            if (IsDaylikeState(currentState))
            {
                // Ya estábamos en un estado “oficial” de día → no hacemos nada
                if (lastOfficialGameState == GameState.Day)
                    return;

                // Si venimos de “Night” o de un estado inválido, cambiamos a dayMusic
                lastOfficialGameState = GameState.Day;
                PlayMusic(dayMusic);
            }
            // Si el estado actual es “Night”
            else if (currentState == GameState.Night)
            {
                // Ya estábamos en un estado “oficial” de noche → no hacemos nada
                if (lastOfficialGameState == GameState.Night)
                    return;

                lastOfficialGameState = GameState.Night;
                PlayMusic(nightMusic);
            }
            // Cualquier otro estado (Paused, OnInventory, OnCrafting, etc.) → no tocamos la pista
        }
    }

    /// <summary>
    /// Reproduce la música correcta al cargar una escena o al iniciar el juego.
    /// </summary>
    private void PlayMusicAccordingToSceneOrState()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // 1) Si es la escena de menú:
        if (sceneName == "MenuScene")
        {
            lastOfficialGameState = GameState.None;
            PlayMusic(menuMusic);
            return;
        }

        // 2) Si es la escena de juego (SampleScene o como la hayas llamado):
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
                    // Si arranca en Paused u otro estado (caso extraño), por defecto reproducimos dayMusic
                    lastOfficialGameState = GameState.Day;
                    PlayMusic(dayMusic);
                }
            }
            else
            {
                // Si todavía no existe GameManager, reproducimos dayMusic por defecto
                lastOfficialGameState = GameState.Day;
                PlayMusic(dayMusic);
            }
        }

        // 3) Si carga alguna otra escena desconocida, no cambiamos nada
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
            // Si ya estaba reproduciendo exactamente ese clip, no hacemos nada.
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();
        Debug.Log($"[MusicManager] Reproduciendo: {clip.name}");
    }
}
