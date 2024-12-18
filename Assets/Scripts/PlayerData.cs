using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public float mouseSensitivity;
    public float volume;
    public bool invertedControls;

    public PlayerData()
    {
        mouseSensitivity = GameManager.mouseSensitivity;
        volume = GameManager.volume;
        invertedControls = GameManager.invertedControls;
    }
}