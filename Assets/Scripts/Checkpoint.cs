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
            
            // if player touches door trigger without raising door first, don't lower again
            if (isDoorTrigger && Door.movingUp)
            {                
                Door.LowerDoors();
                Door.ResetDoorCounter();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
