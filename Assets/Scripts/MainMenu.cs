/*
@Authors - Patrick
@Description - Recycled main menu code from UI lab
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject mainPanel, creditsPanel, directionsPanel;
    private SceneTransfer fadeScript;
    private GameObject fadePanel;

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

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

    public void SwitchMenus(int menuState)
    {
        switch(menuState)
        {
            case 0:
                mainPanel.SetActive(true);
                creditsPanel.SetActive(false);
                directionsPanel.SetActive(false);
                break;
            case 1:
                mainPanel.SetActive(false);
                creditsPanel.SetActive(true);
                directionsPanel.SetActive(false);
                break;
            case 2:
                mainPanel.SetActive(false);
                creditsPanel.SetActive(false);
                directionsPanel.SetActive(true);
                break;
            default:
                break;
        }
    }
}
