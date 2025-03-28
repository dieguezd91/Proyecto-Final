using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Menu : MonoBehaviour
{
    [SerializeField] private Text texto;
    public GameObject currentPanel;
    [SerializeField] Button elBoton;

    private void Start()
    {
        elBoton.interactable = true;
        //Option.interactable = true;
        //back.interactable = true;

    }

    public void OnPlay()
    {
        SceneManager.LoadScene("SampleScene");
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
        LoadingManager.Instance.LoadScene(5, 1);
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

    public void Cerrar()
    {
        Application.Quit();
    }

    public void GoBack()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
