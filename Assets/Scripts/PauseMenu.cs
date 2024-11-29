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

        gameIsPaused = true;
    }

    public void Resume()
    {
        Debug.Log("button has been pressed");
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameManager.gameManager.DisableCursor();

        gameIsPaused = false;
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void TimeBuffer()
    {
        gameManagerProtector = false;
    }
}
