/*
@Authors - Patrick
*/

//!DO NOT mess with Time.timeScale in this class! It will overwrite what pause menu does.
//!If there is an issue with the death menu, also check the pause menu. Sometimes changes to one can screw the other over.

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
    [SerializeField] private GameObject HUD;

    void Start()
    {
        gameManagerProtector = true;
        Invoke("TimeBuffer", 0.5f);
    }

    void LateUpdate(){
        //prevent null reference exception for GameManager
        if (gameManagerProtector || PauseMenu.gameIsPaused){
            return;
        }

        
        if (deathMenuActive)
        {
            EnablePanel();
        }
        else
        {
            DisablePanel();
        }
        
    }

    public void EnablePanel()
    {
        deathMenuUI.
            
            
            (true);
        GameManager.gameManager.MenuMode(HUD);

        deathMenuActive = true;
    }

    public void DisablePanel()
    {
        deathMenuUI.
            
            
            (false);
        GameManager.gameManager.GameplayMode(HUD);

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
