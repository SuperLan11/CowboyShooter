/*
@Authors - Landon, Patrick
@Description - Checkpoint functionality
datafields.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{    
    // checkpoint can also be a door lower trigger
    private bool isDoorTrigger;
    //[SerializeField] private int roomNum;
    //!This variable may be redundant. Ask Landon to see if this variable could just be swapped out with roomNum
    [SerializeField] private int checkpointNum;

    // Start is called before the first frame update
    void Start()
    {
        // mesh renderer makes it easier to see trigger while resizing in scene
        // checkpoint should be on Ignore Raycast layer to not mess with crosshair color
        GetComponent<MeshRenderer>().enabled = false;
        foreach(MeshRenderer meshRender in GetComponentsInChildren<MeshRenderer>())
        {
            meshRender.enabled = false;
        }        

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
            bool reachedNewCheckpoint = (checkpointNum > GameManager.currentCheckpoint);
            if (!reachedNewCheckpoint)
            {
                //Debug.Log("not a new checkpoint, returning");
                return;
            }

            GameManager.currentCheckpoint = checkpointNum;            
            Player.player.SetHealth(Player.player.GetMaxHealth());

            Player.respawnPos = transform.position;
            Player.respawnRot = transform.eulerAngles;
            Player.hasCheckpoint = true;

            // if player touches door trigger without raising door first, don't lower again      
            if (isDoorTrigger && Door.movingUp)
            {                
                Door.LowerDoors();
                Door.ResetDoorCounter();
            }
        }
    }
}