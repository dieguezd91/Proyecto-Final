using UnityEngine;
using UnityEngine.SceneManagement;
using System;

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
    private GameState lastState;

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

        // Creamos el AudioSource en tiempo de ejecución
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0.5f;
        audioSource.mute = false;
    }

    private void Start()
    {
        // Si hay un GameManager en escena, nos suscribimos a su evento
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            lastState = GameManager.Instance.GetCurrentGameState();
        }

        // Reproducir el clip inicial según el estado actual:
        // Si estamos en MenuScene, sonar menuMusic; si no, Day/Night.
        PlayMusicAccordingToSceneOrState();
    }

    private void OnDestroy()
    {
        // Nos desuscribimos al destruir este objeto
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
    }

    /// <summary>
    /// Se ejecuta cada vez que GameManager cambia de GameState.
    /// Aquí elegimos entre dayMusic o nightMusic.
    /// </summary>
    private void HandleGameStateChanged(GameState newState)
    {
        // Si volvemos al menú (MainMenu), reproducimos menuMusic
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "MenuScene")
        {
            PlayMusic(menuMusic);
            lastState = newState;
            return;
        }

        // Solo reaccionamos cuando entramos a Day o a Night
        if (newState == GameState.Day || newState == GameState.Digging || newState == GameState.Harvesting || newState == GameState.Planting || newState == GameState.Removing)
        {
            PlayMusic(dayMusic);
        }
        else if (newState == GameState.Night)
        {
            PlayMusic(nightMusic);
        }

        lastState = newState;
    }

    /// <summary>
    /// Al arrancar, chequea si estamos en la escena de menú o en el juego
    /// y reproduce el clip correspondiente (menu, day o night) según el estado actual.
    /// </summary>
    private void PlayMusicAccordingToSceneOrState()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "MenuScene")
        {
            // Si la escena actual es MenuScene, reproducimos menuMusic
            PlayMusic(menuMusic);
            return;
        }

        // Sino, estamos en la escena de juego, así que vemos el estado actual:
        if (GameManager.Instance != null)
        {
            GameState current = GameManager.Instance.GetCurrentGameState();
            if (current == GameState.Day)
                PlayMusic(dayMusic);
            else if (current == GameState.Night)
                PlayMusic(nightMusic);
            else
                PlayMusic(dayMusic);
            // si por alguna razón el estado arranca en algo distinto, forzamos dayMusic por defecto
        }
    }

    /// <summary>
    /// Método que asigna el clip al AudioSource y lo reproduce.
    /// Si ya estaba con el mismo clip, no hace nada.
    /// </summary>
    private void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[MusicManager] El AudioClip es null. Revisa en el Inspector que hayas asignado un clip válido.");
            return;
        }

        if (audioSource.clip == clip && audioSource.isPlaying)
        {
            // Si ya estaba reproduciendo ese clip, no cambia nada.
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();
        Debug.Log($"[MusicManager] Reproduciendo: {clip.name}");
    }
}
