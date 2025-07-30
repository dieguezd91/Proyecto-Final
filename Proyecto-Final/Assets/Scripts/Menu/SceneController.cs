using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField] Button startGame;

    private void Start()
    {
        startGame.interactable = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnPlay()
    {
        SoundManager.Instance.PlayOneShot("ButtonClick");
        ResetPersistentManagers();
        SceneManager.LoadScene("GameScene");
    }

    public void Restart()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameData();
        }

        ResetPersistentManagers();

        SceneManager.LoadScene("GameScene");
    }

    public void Reset()
    {
        ResetPersistentManagers();
        SceneManager.LoadScene(0);
    }

    public void GoBack()
    {
        ResetPersistentManagers();
        SceneManager.LoadScene("MenuScene");
    }

    private void ResetPersistentManagers()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearAllMaterials();
            InventoryManager.Instance.SetGold(0);
        }

        Time.timeScale = 1f;
    }

    public void Options()
    {
        LoadingManager.Instance.LoadScene(1, 6);
    }

    public void Back()
    {
        LoadingManager.Instance.LoadScene(6, 1);
    }

    public void Next()
    {
        LoadingManager.Instance.LoadScene(5, 3);
    }

    public void CloseGame()
    {
        SoundManager.Instance.PlayOneShot("ButtonClick");
        Application.Quit();
    }
}