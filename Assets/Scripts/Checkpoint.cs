using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool isCollected = false;
    // checkpoint can also be a door lower trigger
    private bool isDoorTrigger;

    // Start is called before the first frame update
    void Start()
    {
        // mesh renderer makes it easier to see trigger while resizing in scene
        GetComponent<MeshRenderer>().enabled = false;

        // means this checkpoint is parented to a door
        if (transform.parent != null)
        {
            isDoorTrigger = true;
            
        }
        else
        {
            isDoorTrigger = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {        
        if (other.gameObject.name == "Player" && !isCollected)
        {            
            isCollected = true;
            Player.roomNum++;
            Player.respawnPos = transform.position;
            Player.hasCheckpoint = true;

            // !Door.movingDown shouldn't happen in actual level
            // if player touches door trigger without raising door first, don't lower again
            if (isDoorTrigger && !Door.movingDown)
            {                
                Door.LowerDoors();
                Door.ResetDoorCounter();
            }
        }
    }

    private void ActivateEnemies()
    {
        // not needed, but if the player can skip some enemies and can't backtrack to kill them, this prevents a softlock
        Enemy.enemiesInRoom = 0;

        // don't think deactivated objects are found with FindObjects        
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            Debug.Log("enemy room num: " + enemy.roomNum);
            if (enemy.roomNum == Player.roomNum)
            {
                enemy.gameObject.SetActive(true);
                Debug.Log(gameObject.name + " now enabled");
                Enemy.enemiesInRoom++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
