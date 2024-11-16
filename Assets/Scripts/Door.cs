using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Door : MonoBehaviour
{
    public bool movingUp = false;
    public bool movingDown = false;

    private Vector3 raisedPos;    
    private Vector3 loweredPos;
    private Vector3 triggerPos;

    // rigidbody needed for collisions to work
    private Rigidbody doorRig;
    private AudioSource doorSlamSfx;
    private bool soundPlayed = false;
    
    // serialized values override script values unless script values are set in Start()
    [SerializeField] private float raiseHeight;
    [SerializeField] private float raiseAccel;
    [SerializeField] private float lowerAccel;

    // Start is called before the first frame update
    void Start()
    {
        doorSlamSfx = GetComponent<AudioSource>();
        triggerPos = transform.GetChild(0).position;

        loweredPos = transform.position;
        raisedPos = transform.position;
        raisedPos.y += raiseHeight;        
    }

    public static void RaiseDoors()
    {
        Door[] doors = FindObjectsOfType<Door>();        
        foreach(Door door in doors)
        {
            door.movingDown = false;
            door.movingUp = true;            
        }
    }

    public static void LowerDoors()
    {
        Door[] doors = FindObjectsOfType<Door>();
        foreach (Door door in doors)
        {
            door.movingUp = false;
            door.movingDown = true;            
        }
    }

    public static void UpdateDoorCounter(int enemiesAlive)
    {        
        GameObject[] doors = GameObject.FindGameObjectsWithTag("DOOR");
        foreach (GameObject door in doors)
        {
            door.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = enemiesAlive.ToString();
        }     
    }

    // Update is called once per frame
    void Update()
    {        
        if (movingUp)
        {            
            transform.position = Vector3.Lerp(transform.position, raisedPos, raiseAccel);
            // door trigger is child, don't let parent move it from ground
            transform.GetChild(0).position = triggerPos;
            soundPlayed = false;
        }
        else if (movingDown)
        {
            if (!soundPlayed)
            {
                doorSlamSfx.Play();
                soundPlayed = true;
            }
            transform.position = Vector3.Lerp(transform.position, loweredPos, lowerAccel);
            transform.GetChild(0).position = triggerPos;
        }
    }
}
