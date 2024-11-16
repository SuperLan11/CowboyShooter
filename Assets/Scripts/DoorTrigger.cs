using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// door trigger also acts as a checkpoint / respawn point
public class DoorTrigger : MonoBehaviour
{
    // the room number starting at 1 that represents the room the player will respawn in
    [SerializeField] private int roomNum;

    // Start is called before the first frame update
    void Start()
    {
        // mesh renderer is on trigger so it is easier to see while resize
        GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player")
        {            
            Player.roomNum = roomNum;
            // shouldn't happen in actual level, if player touches door trigger without raising door first, don't lower again
            if(!Door.movingDown)
                Door.LowerDoors();
            // fix this later
            Door.UpdateDoorCounter(Enemy.enemiesInitialized);
            Player.respawnPos = transform.position;
            Player.hasCheckpoint = true;            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
