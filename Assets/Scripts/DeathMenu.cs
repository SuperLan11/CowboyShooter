using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class DeathMenu : MonoBehaviour
{
    public static bool deathMenuActive = false;
    private bool gameManagerProtector;
    [SerializeField] private GameObject deathMenuUI;
    [SerializeField] private GameObject crosshair;

    void Start()
    {
        gameManagerProtector = true;
        Invoke("TimeBuffer", 0.5f);
    }

    void LateUpdate(){
        //prevent null reference exception for GameManager
        if (gameManagerProtector){
            return;
        }

        /*
        if (deathMenuActive)
        {
            EnablePanel();
        }
        else
        {
            DisablePanel();
        }
        */
    }

    public void EnablePanel()
    {
        deathMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameManager.gameManager.EnableCursor();
        crosshair.GetComponent<Crosshair>().DisableCrosshair();

        deathMenuActive = true;
    }

    public void DisablePanel()
    {
        deathMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameManager.gameManager.DisableCursor();
        crosshair.GetComponent<Crosshair>().EnableCrosshair();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        deathMenuActive = false;
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
        GameManager.gameManager.CleanupScene();
    }

    public void QuitGame()
    {
        GameManager.QuitGame();
        //Application.Quit();
    }

    private void TimeBuffer()
    {
        gameManagerProtector = false;
    }
}
