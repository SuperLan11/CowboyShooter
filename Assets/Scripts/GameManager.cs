/*
@Authors - Patrick
@Description - Class that manages game data that needs to be shared between other classes. It could just go in player because
that's a singleton as well, but we want to keep the scripts concise.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    
    //doesn't look like it's currently being used for anything. We'll need to change that if we need to do more complicated stuff
    //with scene management
    public int currentLevel;
    public static int currentCheckpoint = 0;
    [SerializeField] private bool debugMode = false;
    private bool disableCursor;
    private bool enableCursor;
    public static double storedTime;
    public static float mouseSensitivity = 2f;

    public void Awake()
    {
        gameManager = this; 

        //makes obj persistent through checkpoints and scene transitions
        DontDestroyOnLoad(this.gameObject); 
    }

    //!You MUST change game manager variables here!
    public void Start()
    {
        //Debug.Log("cp on Start(): " + currentCheckpoint);
        disableCursor = false;
        enableCursor = false;

        storedTime = 0;
    }

    public void Update()
    {
        if (debugMode)
        {
            Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();
            foreach (Enemy enemy in enemies)
            {
                Destroy(enemy.gameObject);
            }
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
        storedTime = time;
    }

    public void ResetTimerValue()
    {
        storedTime = 0;
    }

    public void DestroySelf()
    {
        Destroy(this.gameObject);
    }
}
