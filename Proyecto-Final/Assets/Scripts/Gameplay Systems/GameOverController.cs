using UnityEngine;
using System.Collections;

public class GameOverController : MonoBehaviour
{
    [SerializeField] private float gameOverDelay = 2f;
    [SerializeField] private GameObject home;
    [SerializeField] private PauseController pauseController;

    private HouseLifeController homeLife;
    private GameResetController gameResetController;

    private void Awake()
    {
        gameResetController = GetComponent<GameResetController>();
        if (gameResetController == null)
        {
            Debug.LogError("GameResetController missing on GameOverController!");
        }

        if (pauseController == null)
        {
            pauseController = FindObjectOfType<PauseController>();
        }

        if (home == null)
        {
            home = GameObject.FindGameObjectWithTag("Home");
        }

        if (home != null)
        {
            homeLife = home.GetComponent<HouseLifeController>();
        }
    }

    private void Start()
    {

        if (homeLife != null)
        {
            homeLife.onHouseDestroyed.AddListener(HandleHomeDeath);
        }
        // Re-bind buttons dynamically due to cross-prefab serialization issues
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.gameOverPanel != null)
            {
                UnityEngine.UI.Button[] buttons = UIManager.Instance.gameOverPanel.GetComponentsInChildren<UnityEngine.UI.Button>(true);
                foreach (UnityEngine.UI.Button btn in buttons)
                {
                    TMPro.TextMeshProUGUI tmp = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
                    if (tmp != null)
                    {
                        string txt = tmp.text.ToLower();
                        if (txt.Contains("menu") || txt.Contains("menú"))
                            btn.onClick.AddListener(GameOverMainMenu);
                        else if (txt.Contains("restart") || txt.Contains("reiniciar") || txt.Contains("retry") || txt.Contains("reintentar"))
                            btn.onClick.AddListener(GameOverRestart);
                    }
                }
            }
            if (UIManager.Instance.continuePanel != null)
            {
                UnityEngine.UI.Button[] buttons = UIManager.Instance.continuePanel.GetComponentsInChildren<UnityEngine.UI.Button>(true);
                foreach (UnityEngine.UI.Button btn in buttons)
                {
                    TMPro.TextMeshProUGUI tmp = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
                    if (tmp != null && (tmp.text.ToLower().Contains("menu") || tmp.text.ToLower().Contains("menú")))
                    {
                        btn.onClick.AddListener(GameOverMainMenu);
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (homeLife != null)
        {
            homeLife.onHouseDestroyed.RemoveListener(HandleHomeDeath);
        }
    }

    private void HandleHomeDeath()
    {
        StartCoroutine(ShowGameOverAfterDelay());
    }

    private IEnumerator ShowGameOverAfterDelay()
    {
        yield return new WaitForSeconds(gameOverDelay);

        if (GameFlowController.Instance != null)
            GameFlowController.Instance.SetPhase(GamePhase.GameOver);

        if (UIManager.Instance != null && UIManager.Instance.gameOverPanel != null)
        {
            UIManager.Instance.gameOverPanel.SetActive(true);

            if (pauseController != null) pauseController.Pause();
            else Time.timeScale = 0f;
        }
    }

    public void ShowContinuePanel()
    {
        if (GameFlowController.Instance != null)
            GameFlowController.Instance.SetPhase(GamePhase.GameOver);

        if (UIManager.Instance != null && UIManager.Instance.continuePanel != null)
        {
            UIManager.Instance.StartCoroutine(UIManager.Instance.AnimateContinuePanel());
            if (pauseController != null) pauseController.Pause();
            else Time.timeScale = 0f;
        }
        else
        {
            Debug.LogWarning("No se encontró el panel de 'Continuar' en el UIManager.");
        }
    }

    public void GameOverRestart()
    {
        if (pauseController != null) pauseController.Resume();
        else Time.timeScale = 1f;
        if (GameFlowController.Instance != null)
            GameFlowController.Instance.SetPhase(GamePhase.Day);
            
        try { if (gameResetController != null) gameResetController.ResetGame(); } catch (System.Exception e) { Debug.LogError("Error in ResetGame: " + e); }
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void GameOverMainMenu()
    {
        if (pauseController != null) pauseController.Resume();
        else Time.timeScale = 1f;
        if (GameFlowController.Instance != null)
            GameFlowController.Instance.SetPhase(GamePhase.Day);
            
        try { if (gameResetController != null) gameResetController.ResetGame(); } catch (System.Exception e) { Debug.LogError("Error in ResetGame: " + e); }
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}






