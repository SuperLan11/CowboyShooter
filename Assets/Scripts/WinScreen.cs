

using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinScreen : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    
    // Start is called before the first frame update
    void Start()
    {
        double finalTime = GameManager.totalTime;

        double seconds = System.Math.Round(finalTime % 60, 2);
        int minutes = (int)(finalTime / 60) % 60;

        string tempString = "   " + (minutes.ToString() + " minutes and " + seconds.ToString() + " seconds!");

        timerText.text += tempString;

        //Debug.Log("good morning");
        //Invoke("LoadMainMenu", 5);
    }

    public void LoadMainMenu()
    {
        GameManager.gameManager.DestroySelf();
        SceneManager.LoadScene(0);
    }
}
