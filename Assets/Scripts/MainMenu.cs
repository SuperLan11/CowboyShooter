/*
@Authors - Patrick
@Description - Recycled main menu code from UI lab
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainPanel, creditsPanel, directionsPanel;

    public void startGamePressed(){
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    public void switchMenus(int menuState){
        switch(menuState){
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
