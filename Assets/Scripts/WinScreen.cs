using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class WinScreen : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public VideoPlayer videoPlayer;
    public GameObject winScreenUI;
    public GameObject cutsceneScreen;
    public Camera gameCamera;
    public GameObject gameMusic;
    public GameObject environment;

    [System.NonSerialized] public bool skippedCutscene = false;
    [System.NonSerialized] public bool cutsceneCompleted = false;

    void Start()
    {
        StartCoroutine(WaitForVideoToLoad());
    }

    public IEnumerator WaitForVideoToLoad()
    {
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(WaitForCutscene());
    }

    public IEnumerator WaitForCutscene()
    {
        while (videoPlayer.isPlaying && !skippedCutscene)
        {
            yield return null;
        }

        if (!cutsceneCompleted)
        {
            StartWinScreen();
        }
    }

    public void StartWinScreen()
    {
        if (cutsceneCompleted)
            return;

        cutsceneCompleted = true;
        RestoreWinScreen();

        GameManager.gameManager.EnableCursor();

        double finalTime = GameManager.totalTime;
        int seconds = (int)finalTime % 60;
        int minutes = (int)(finalTime / 60);

        string tempString = "   " + (minutes.ToString() + " minutes and " + seconds.ToString() + " seconds!");
        timerText.text += tempString;
    }

    private void RestoreWinScreen()
    {
        Destroy(videoPlayer.gameObject);
        winScreenUI.SetActive(true);
        cutsceneScreen.SetActive(false);
        gameCamera.clearFlags = CameraClearFlags.Skybox;
        gameMusic.SetActive(true);
        environment.SetActive(true);
    }

    public void LoadMainMenu()
    {
        GameManager.gameManager.DestroySelf();
        SceneManager.LoadScene(0);
    }
}
