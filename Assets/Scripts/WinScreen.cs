

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinScreen : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    
    // Start is called before the first frame update
    void Start()
    {
        //!put player time code here
        timerText.text += GameManager.totalTime.ToString();

        //Debug.Log("good morning");
        //Invoke("LoadMainMenu", 5);
    }

    public void LoadMainMenu()
    {
        GameManager.gameManager.DestroySelf();
        SceneManager.LoadScene(0);
    }
}
