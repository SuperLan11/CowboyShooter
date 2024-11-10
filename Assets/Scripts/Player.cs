/*
@Authors - Patrick, Landon
@Authors - Patrick, Landon
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

    private bool tryingToJump;
    public float jumpStrength = 7f;

    private Camera cam;
    private GameObject lasso;

    private AudioSource gunSfx;

    // IMPORTANT! If you assign these values here, they must be the same as the inspector
    // Otherwise, movement is reversed
    [SerializeField] float horRotSpeed;
    [SerializeField] float vertRotSpeed;

    public enum movementState
    {
        GROUND,
        AIR,
        SWINGING,
        HANGING,
        WALL
    };

    public movementState currentMovementState;

    void Start()
    {
        // this makes the cursor stay insivible in the editor
        // to make cursor visible, press Escape  
        Cursor.lockState = CursorLockMode.Locked;      
        Cursor.visible = false;

        cam = Camera.main;
        // lasso should be the second child of Camera for this to work
        lasso = transform.GetChild(0).GetChild(1).gameObject;        
        rigidbody = GetComponent<Rigidbody>();        
    }

    //!Implement this ASAP!
    protected void Shoot()
    {        
        if (gunSfx != null)
            gunSfx.Play();

        // hitscan logic

        // damage logic
    }

    protected void Lasso()
    {
        
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
        if (context.started)
        {
            Shoot();
        }
    }

    public void LassoActivated(InputAction.CallbackContext context)
    {        
        if (context.started)
        {
            player.currentMovementState = movementState.SWINGING;
            lasso.GetComponent<Lasso>().StartLasso();
        }
        else if (context.canceled)
        {
            if (player.currentMovementState != movementState.GROUND)
            {
                Debug.Log("Prev state: " + player.currentMovementState);
                player.currentMovementState = movementState.AIR;
            }

            lasso.GetComponent<Lasso>().EndLasso();
            //Debug.Log("STOPPED HOLDING RMB");
        }
    }

    public void JumpActivated(InputAction.CallbackContext context)
    {
        Debug.Log("movementState: " + player.currentMovementState);
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
        bool hitBeneath, onFloor;
        for (int i = 0; i < collision.contactCount; i++)
        {
            hitBeneath = collision.GetContact(i).point.y < transform.position.y;
            onFloor = collision.GetContact(i).otherCollider.gameObject.tag == "FLOOR";
            
            // if player hits something beneath them, they hit floor
            if (hitBeneath && onFloor)
            {
                currentMovementState = movementState.GROUND;
                //!without break, movement state is determined by last contact
                break;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
    }

    void FixedUpdate()
    {        
        // wall jumping?

        // need to assign y velocity first so it is not overriden
        rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0);

        // moves player left/right/forward/backward from direction they're facing
        rigidbody.velocity += (transform.right * lastMoveInput.x +
                             transform.forward * lastMoveInput.y) * speed;

        // Input.GetAxis is the change in value since last frame
        float deltaMouseX = Input.GetAxis("Mouse X");
        float deltaMouseY = -Input.GetAxis("Mouse Y");

        // Player will not scroll vertically so that transform.forward doesn't move into the sky
        Vector3 playerRot = transform.rotation.eulerAngles;
        playerRot.y += deltaMouseX * horRotSpeed;
        // just in case
        playerRot.x = 0;
        playerRot.z = 0;
        transform.eulerAngles = playerRot;

        // later: make vertical threshold camera cannot scroll past (at feet and in sky)

        // The camera only scrolls vertically since the player parent object handles horizontal scroll
        Vector3 camRot = cam.transform.rotation.eulerAngles;
        camRot.x += deltaMouseY * vertRotSpeed;                       
        camRot.z = 0;
        cam.transform.eulerAngles = camRot;

        if (tryingToJump && isGrounded())
        {
            //Debug.Log("jumping");
            rigidbody.velocity += new Vector3(0, jumpStrength, 0);
            tryingToJump = false;
            currentMovementState = movementState.AIR;
        }
    }

    public bool isGrounded(){
        return (currentMovementState == movementState.GROUND);
    }
}
