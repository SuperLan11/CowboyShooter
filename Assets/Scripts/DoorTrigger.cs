using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
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
            int roomNum = FindObjectsOfType<Door>().Length;
            Player.player.roomNum = roomNum;
            Door.LowerDoors();
            Door.UpdateDoorCounter(Enemy.enemiesInitialized);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
