using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public static bool movingUp = false;
    public static bool movingDown = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public static void RaiseDoors()
    {
        Door[] doors = FindObjectsOfType<Door>();
        foreach(Door door in doors)
        {

        }
    }

    // make a door trigger script later that lowers all doors when touched

    public static void LowerDoors()
    {
        Door[] doors = FindObjectsOfType<Door>();
        foreach (Door door in doors)
        {

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
