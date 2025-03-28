using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuPausa : MonoBehaviour
{

    //[SerializeField] private GameObject botonPausa;

    [SerializeField] private GameObject menuPausa;

    private bool isPaused = false;
    public static bool isGamePaused = false;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePaused)
            {
                Reanudar();
            }
            else
            {
                Pausa();
            }
        }
    }

    public void Pausa()
    {
        Time.timeScale = 0f;
        menuPausa.SetActive(true);

        isGamePaused = true;
    }

    public void Reanudar()
    {
        Time.timeScale = 1f;
        menuPausa.SetActive(false);

        isGamePaused = false;
    }

    public void Cerrar()
    {
        Time.timeScale = 1f;
        menuPausa.SetActive(false);

        isGamePaused = false;
        SceneManager.LoadScene("MenuScene");

    }

    //public void Restart()
    //{
    //    Time.timeScale = 1f;
    //    LoadingManager.Instance.LoadScene(3, 1);

    //}

    //public void Restart1()
    //{
    //    Time.timeScale = 1f;
    //    LoadingManager.Instance.LoadScene(2, 1);
    //}

    //public void Restart3()
    //{
    //    Time.timeScale = 1f;
    //    LoadingManager.Instance.LoadScene(4, 1);
    //}
}
