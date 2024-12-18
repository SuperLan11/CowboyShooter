/*
@Authors - Patrick
*/

//!If there is an issue with the pause menu, also check the death menu. Sometimes changes to one can screw the other over.

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;


public class PauseMenu : MonoBehaviour
{
    public static bool gameIsPaused = false;
    public static bool inOptionsMenu = false;
    private bool gameManagerProtector;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject optionsMenuUI;
    [SerializeField] private GameObject HUD;
    [SerializeField] private AudioMixer masterVolume;
    private const string masterVolumeString = "MasterVolume";

    void Start()
    {
        gameManagerProtector = true;
        Invoke("TimeBuffer", 0.5f);
    }

    void LateUpdate(){
        //prevent null reference exception for GameManager
        if (gameManagerProtector || DeathMenu.deathMenuActive || GameManager.gameManager.gameIsEnding){
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
        pauseMenuUI.SetActive(!inOptionsMenu);
        Time.timeScale = 0f;
        GameManager.gameManager.MenuMode(HUD);

        gameIsPaused = true;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameManager.gameManager.GameplayMode(HUD);

        gameIsPaused = false;
    }

     public void SwitchMenus(int menuState)
    {
        switch(menuState)
        {
            case 0:
                pauseMenuUI.SetActive(true);
                optionsMenuUI.SetActive(false);
                inOptionsMenu = false;
                Player.player.SetPlayerMouseSensitivity();
                break;
            case 1:
                pauseMenuUI.SetActive(false);
                optionsMenuUI.SetActive(true);
                inOptionsMenu = true;
                break;
            default:
                break;
        }
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

    public void SetVolume(float volume)
    {
        //Unity doesn't handle audio linearly
        float volumeAdjustedForAudioEquation = Mathf.Log10(volume) * 20f;
        masterVolume.SetFloat(masterVolumeString, volumeAdjustedForAudioEquation);
        GameManager.volume = volumeAdjustedForAudioEquation;
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        //this allows us to make the slider 1 to 100 instead of 1 to 20, which is more apealing imo
        float conversionRatio = 5f;
        float adjustedSensitivity = sensitivity / conversionRatio;
        GameManager.mouseSensitivity = adjustedSensitivity;
    }
}
