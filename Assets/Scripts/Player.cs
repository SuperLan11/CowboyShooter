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
    public float acceleration = 10f;

    private bool canJump;
    private bool tryingToJump;
    public float jumpStrength = 7f;
    private bool grounded = false;

    private Camera cam;
    private Vector3 camOffset;
    public float horRotSpeed = 2f;
    public float vertRotSpeed = 3f;

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
        // this makes the cursor stay insivible in the editor
        // uncomment this when game is done, or find a way to turn off in editor        
        //Cursor.visible = false;

        cam = Camera.main;
        rigidbody = GetComponent<Rigidbody>();        
    }

    //!Implement this ASAP!
    protected void Shoot()
    {
        //shoot logic
    }

    private void Awake()
    {
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

    public void MoveActivated(InputAction.CallbackContext context)
    {
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

    private void OnCollisionEnter(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            // if player hits something beneath them, they hit floor
            if (collision.GetContact(i).point.y < transform.position.y)
            {
                grounded = true;
                // without break, grounded is determined by last contact
                break;
            }

        }
    }

    void Update()
    {        
        // need to assign y velocity first so it is not overriden
        rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0);

        // moves player left/right/forward/backward from direction they're facing
        rigidbody.velocity += (transform.right * lastMoveInput.x +
                             transform.forward * lastMoveInput.y) * speed;

        // Input.GetAxis is the change in value since last frame
        float deltaMouseX = Input.GetAxis("Mouse X");
        float deltaMouseY = -Input.GetAxis("Mouse Y");

        Debug.Log("deltaMouseX: " + deltaMouseX);
        Debug.Log("deltaMouseY: " + deltaMouseY);

        // Player will not scroll vertically so that transform.forward doesn't move into the sky
        Vector3 playerRot = transform.rotation.eulerAngles;
        playerRot.y += deltaMouseX;
        transform.eulerAngles = playerRot;

        // later: make vertical threshold camera cannot scroll past (at feet and in sky)

        // The camera only scrolls vertically since the player parent object handles horizontal scroll
        Vector3 camRot = cam.transform.rotation.eulerAngles;        
        camRot.x += deltaMouseY;        
        // make z rotation 0 to avoid barrel roll in case some weird collision happens
        camRot.z = 0;
        cam.transform.eulerAngles = camRot;

        if (tryingToJump && grounded)
        {
            rigidbody.velocity += new Vector3(0, jumpStrength, 0);
            tryingToJump = false;
            grounded = false;
        }
    }
}
