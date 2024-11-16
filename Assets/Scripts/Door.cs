using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Door : MonoBehaviour
{
    public static bool movingUp = false;
    public static bool prevMovingUp = false;
    public static bool movingDown = false;

    private Vector3 raisedPos;    
    private Vector3 loweredPos;
    private Vector3 triggerPos;

    // rigidbody needed for collisions to work
    private Rigidbody doorRig;
    private AudioSource doorSlamSfx;
    // static so multiple door sounds don't play at once
    private static bool soundPlayed = false;
    
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

    // a function isn't required to access these variables, but it is more readable
    public static void RaiseDoors()
    {        
        movingDown = false;
        movingUp = true;                    
    }

    public static void LowerDoors()
    {        
        movingUp = false;
        movingDown = true;                    
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
            movingDown = false;
            transform.position = Vector3.Lerp(transform.position, raisedPos, raiseAccel);
            // door trigger is child, don't let parent move it from ground
            transform.GetChild(0).position = triggerPos;
            soundPlayed = false;
        }
        // wasMovingUp
        else if (movingDown)
        {
            movingUp = false;
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
