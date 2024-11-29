/*
@Authors - Patrick
@Description - Class that makes sure there's only one GameManager. Necessary because GameManager needs to be spawned at any
scene coming from level select, so this logic can't go in GameManager.cs
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject gameManagerPrefab;

    //variables for newly instantiated GameManager
    [SerializeField] private bool debugMode = false;
    public int currentLevel;
    
    private void LateUpdate()
    {
        //!there should only ever be one game manager in a scene
        GameManager manager = FindObjectOfType<GameManager>();

        if (manager == null)
        {
            GameObject tempObj = Instantiate(gameManagerPrefab);
            
            manager = tempObj.GetComponent<GameManager>();
        }

        manager.debugMode = debugMode;
        manager.currentLevel = currentLevel;;
    }
}