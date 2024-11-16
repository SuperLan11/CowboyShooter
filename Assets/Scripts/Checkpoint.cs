using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // the room number starting at 1 that represents the room the player will respawn in
    [SerializeField] private int roomNum;
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
        if (other.gameObject.name == "Player")
        {            
            Player.roomNum = roomNum;                        
            Player.respawnPos = transform.position;
            Player.hasCheckpoint = true;

            // shouldn't happen in actual level, if player touches door trigger without raising door first, don't lower again
            if (isDoorTrigger && !Door.movingDown)
            {
                Door.LowerDoors();
                // fix this later
                Door.UpdateDoorCounter(Enemy.enemiesInitialized);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
