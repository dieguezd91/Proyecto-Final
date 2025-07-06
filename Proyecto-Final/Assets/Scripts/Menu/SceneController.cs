using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class SceneController : MonoBehaviour
{
    [SerializeField] Button startGame;

    private void Start()
    {
        startGame.interactable = true;
        //Option.interactable = true;
        //back.interactable = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

    }

    public void OnPlay()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void Options()
    {
        LoadingManager.Instance.LoadScene(1, 6);
    }

    public void Back()
    {
        LoadingManager.Instance.LoadScene(6, 1);
    }

    public void Restart()
    {
        GameManager.Instance.ResetGameData();
        SceneManager.LoadScene("GameScene");
    }

    public void Reset()
    {
        //LoadingManager.Instance.LoadScene(7, 1);
        SceneManager.LoadScene(0);
    }

    public void Next()
    {
        LoadingManager.Instance.LoadScene(5, 3);
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void GoBack()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
