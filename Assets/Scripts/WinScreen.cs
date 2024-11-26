

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameManager.gameManager.DestroySelf();

        Debug.Log("good morning");

        Invoke("LoadMainMenu", 5);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
