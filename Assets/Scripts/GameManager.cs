/*
@Authors - Patrick
@Description - Class that manages game data that needs to be shared between other classes. It could just go in player because
that's a singleton as well, but we want to keep the scripts concise.
*/

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    
    //doesn't look like it's currently being used for anything. We'll need to change that if we need to do more complicated stuff
    //with scene management
    public int currentLevel;
    public bool debugMode = false;
    public static int currentCheckpoint = 0;
    private bool disableCursor;
    private bool enableCursor;
    public static double levelTime;
    public static double totalTime;
    public static float mouseSensitivity = 2f;
    public static float volume = 1f;
    public Enemy[] originalEnemyList;

    public void Awake()
    {
        gameManager = this; 

        //makes obj persistent through checkpoints and scene transitions
        DontDestroyOnLoad(this.gameObject); 
    }

    //!You MUST change game manager variables here!
    public void Start()
    {
        PlayerData playerData = SaveSystem.LoadPlayer();

        if (playerData != null)
        {
            mouseSensitivity = playerData.mouseSensitivity;
            volume = playerData.volume;
        }
        
        //Debug.Log("cp on Start(): " + currentCheckpoint);
        disableCursor = false;
        enableCursor = false;

        levelTime = 0;
        
        if (currentLevel == 0)
        {
            totalTime = 0;
        }
    }

    public void Update()
    {
        if (debugMode)
        {
            Enemy[] enemies = GetAllEnemies();
            DestroyAllEnemies(enemies);
        }

        //This cannot be if-else, they are not mutually exclusive
        if (enableCursor)
        {
            EnableCursor();
        }
        if (disableCursor) 
        {
            DisableCursor();
        }
    }

    public void EnableCursor() 
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void DisableCursor() 
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void StoreTimerValue(double time)
    {
        levelTime = time;
    }

    public void ResetTimerValue()
    {
        levelTime = 0;
    }

    public Enemy[] GetAllEnemies()
    {
        return GameObject.FindObjectsOfType<Enemy>();
    }

    public ExplodingBarrel[] GetAllBarrels()
    {
       return GameObject.FindObjectsOfType<ExplodingBarrel>();
    }

    public void DestroyAllEnemies(Enemy[] enemies)
    {
        foreach (Enemy enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
    }

    public void DestroySelf()
    {
        Destroy(this.gameObject);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //!needs to be called for main menu
    public void CleanupScene()
    {
        // reset floorsInitialized so floors can reset offmeshlinks
        Floor.floorsInitialized = 0;
        Enemy.enemiesInitialized = 0;
        Enemy.enemiesInRoom = 0;
        Door.movingUp = false;
    }

    public void MenuMode(GameObject pHUD)
    {
        EnableCursor();
        pHUD.GetComponent<PlayerHUD>().DisableHUD();
    }

    public void GameplayMode(GameObject pHUD)
    {
        DisableCursor();
        pHUD.GetComponent<PlayerHUD>().EnableHUD();
    }

    public static void QuitGame()
    {
        SaveSystem.savePlayer();
        Application.Quit();
    }
}