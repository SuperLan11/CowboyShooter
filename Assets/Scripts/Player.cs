/*
@Authors - Patrick
@Description - Player singleton class
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Player : Character
{
    //singleton obj that's accessible to all objects
    public static Player player;
    
    private Vector2 lastMoveInput = Vector2.zero;
    private Vector3 maxSpeed = new Vector3(10f, 10f, 10f);
    private float acceleration = 10f;
    private bool canJump;
    private bool tryingToJump;

    private Camera cam;
    private Vector3 camOffset;

    private Rigidbody myRig;

    private enum movementState 
    {
        GROUND,
        AIR,
        SWINGING,
        HANGING,
        WALL
    };

    private movementState currentMovementState;

    void Start()
    {
        cam = Camera.main;
        myRig = GetComponent<Rigidbody>();
        camOffset = cam.transform.position - transform.position;
    }    

    //!Implement this ASAP!
    protected void Shoot(){
        //shoot logic
    } 

    private void Awake() { 
        //Deletes itself if there's another instance. Basically forces the class to be a singleton
        if (player != null && player != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            player = this; 
        } 
    }    

    public void MoveActivated(InputAction.CallbackContext context){
        if (context.started || context.performed)
        {
            lastMoveInput = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            lastMoveInput = Vector2.zero;
        }
    }

    public void ShootActivated(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            Shoot();
        }
    }

    public void LassoActivated(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            //is lasso gonna be a function?
        }
    }

    public void JumpActivated(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            tryingToJump = true;
        }
        else if (context.canceled)
        {
            tryingToJump = false;
        }
    }

    void Update()
    {
        cam.transform.position = transform.position;

        myRig.velocity = new Vector3(0, myRig.velocity.y, 0);
        myRig.velocity += new Vector3(lastMoveInput.x, 0, lastMoveInput.y) * speed;
        
    }
}
