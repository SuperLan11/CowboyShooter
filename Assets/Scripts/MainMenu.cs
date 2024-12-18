/*
@Authors - Patrick and Landon
@Description - Recycled main menu code from UI lab
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MainMenu : MonoBehaviour
{
    public GameObject mainPanel, creditsPanel, directionsPanel, optionsPanel, levelSelectPanel, invertedControlsToggle;
    private SceneTransfer fadeScript;
    private GameObject fadePanel;
    [SerializeField] private AudioMixer masterVolume;
    private const string masterVolumeString = "MasterVolume";

    private void Start()
    {
        StartCoroutine(InitializeVolume());
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Screen.fullScreen = true;

        // get reference to fade panel so it can be called when deactived        
        fadePanel = GameObject.Find("FadePanel");
        fadeScript = fadePanel.GetComponent<SceneTransfer>();
        // if you want a fade in to the main menu, comment this out    
        fadePanel.GetComponent<Image>().enabled = false;
    }

    public void StartGamePressed()
    {
        fadePanel.SetActive(true);
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;        
        StartCoroutine(fadeScript.FadeToNewScene(currentSceneIndex + 1, 1f));
    }


    public void ExitGamePressed()
    {
        GameManager.QuitGame();
        //Application.Quit();
    }

    private IEnumerator InitializeVolume()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        //already adjusted for Unity's nonlinear audio
        masterVolume.SetFloat(masterVolumeString, GameManager.volume);
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

    public void SetInvertedControls(bool inverted)
    {
        GameManager.invertedControls = inverted;
    }

    //I'm sure there's a better way to do this, but who cares
    public void SwitchMenus(int menuState)
    {
        switch(menuState)
        {
            case 0:
                mainPanel.SetActive(true);
                creditsPanel.SetActive(false);
                directionsPanel.SetActive(false);
                optionsPanel.SetActive(false);
                levelSelectPanel.SetActive(false);
                break;
            case 1:
                mainPanel.SetActive(false);
                creditsPanel.SetActive(true);
                directionsPanel.SetActive(false);
                optionsPanel.SetActive(false);
                levelSelectPanel.SetActive(false);
                break;
            case 2:
                mainPanel.SetActive(false);
                creditsPanel.SetActive(false);
                directionsPanel.SetActive(true);
                optionsPanel.SetActive(false);
                levelSelectPanel.SetActive(false);
                break;
            case 3:
                mainPanel.SetActive(false);
                creditsPanel.SetActive(false);
                directionsPanel.SetActive(false);
                optionsPanel.SetActive(true);
                levelSelectPanel.SetActive(false);

                //makes sure inverted controls checkbox is set to correct one
                invertedControlsToggle.GetComponent<Toggle>().isOn = GameManager.invertedControls;
                
                break;
            case 4:
                mainPanel.SetActive(false);
                creditsPanel.SetActive(false);
                directionsPanel.SetActive(false);
                optionsPanel.SetActive(false);
                levelSelectPanel.SetActive(true);
                break;
            default:
                break;
        }
    }

    //I'm sure there's a better way to do this, but who cares
    public void LevelSelect(int levelNum)
    {
        //GameManager.gameManager.currentLevel = levelNum;
        SceneManager.LoadScene(levelNum);
    }
}
