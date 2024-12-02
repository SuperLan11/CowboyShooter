using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PauseMenu : MonoBehaviour
{
    public static bool gameIsPaused = false;
    private bool gameManagerProtector;
    [SerializeField] private GameObject pauseMenuUI;
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

        
        if (gameIsPaused)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameManager.gameManager.EnableCursor();
        crosshair.GetComponent<Crosshair>().DisableCrosshair();

        gameIsPaused = true;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameManager.gameManager.DisableCursor();
        crosshair.GetComponent<Crosshair>().EnableCrosshair();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        gameIsPaused = false;
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
