/*
@Authors - Patrick, Landon
@Authors - Patrick, Landon
@Description - Player singleton class
*/

using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    private bool inJumpCooldown = false;
    public float jumpStrength = 7f;
    private int jumpCooldown;
    private int maxJumpCooldown;

    private Camera cam;
    private GameObject lasso;

    private AudioSource gunSfx;
    [SerializeField] private AudioSource lassoSfx;

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

        jumpCooldown = maxJumpCooldown = 2;     
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
   
    protected void Shoot(GameObject enemy)
    {
        if (gunSfx != null)
            gunSfx.Play();

        enemy.GetComponent<Enemy>().TakeDamage(1);
    }

    GameObject ObjAimedAt()
    {
        RaycastHit hit;
        //Vector3 direction = (Camera.main.transform.position - transform.position).normalized;        
        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity))
        {                    
            return hit.transform.gameObject;
        }
        return null;
    }

    public void ShootActivated(InputAction.CallbackContext context)
    {
        GameObject objAimed = ObjAimedAt();
        Debug.Log("objAimed: " + objAimed.name);
        if (context.started && objAimed.GetComponent<Enemy>() != null)
        {
            Shoot(objAimed);
        }
    }

    public void LassoActivated(InputAction.CallbackContext context)
    {        
        if (context.started)
        {
            bool valid = lasso.GetComponent<Lasso>().StartLasso();

            if (valid){
                player.currentMovementState = movementState.SWINGING;
                lassoSfx.Play();
            }
        }
        else if (context.canceled){
            //prevents player from being in air state after just tapping RMB
            if (player.currentMovementState != movementState.GROUND){
                player.currentMovementState = movementState.AIR;
            }
            
            lasso.GetComponent<Lasso>().EndLasso();
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

    public bool isGrounded()
    {
        return (currentMovementState == movementState.GROUND);
    }

    protected override void Death()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void FixedUpdate()
    {
        //Debug.Log(currentMovementState);
        // wall jumping?

        //forces camera to look straight as you're opening up scene
        if (Time.timeSinceLevelLoad < 0.1f) {
            return;
        }

        // need to assign y velocity first so it is not overriden
        rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0);

        // moves player left/right/forward/backward from direction they're facing
        rigidbody.velocity += (transform.right * lastMoveInput.x +
                             transform.forward * lastMoveInput.y) * speed;

        // Input.GetAxis is the change in value since last frame
        // MouseY is flipped since xRot is negative upwards
        float deltaMouseX = Input.GetAxis("Mouse X");
        float deltaMouseY = -Input.GetAxis("Mouse Y");

        // Player will not scroll vertically so that transform.forward doesn't move into the sky
        Vector3 playerRot = transform.rotation.eulerAngles;
        playerRot.y += deltaMouseX * horRotSpeed;
        // just in case
        playerRot.x = 0;
        playerRot.z = 0;
        transform.eulerAngles = playerRot;

        // The camera only scrolls vertically since the player parent object handles horizontal scroll
        Vector3 camRot = cam.transform.rotation.eulerAngles;

        // camRot.x goes starts decreasing from 360 when you look up and is positive downwards
        // because deltaMouseY is flipped, positive means moving down and vice versa
        bool inNormalRange = (camRot.x > 280f || camRot.x < 80f);
        bool inLowerRange = (camRot.x >= 80f && camRot.x <= 90f && deltaMouseY < -0.001f);
        bool inRaiseRange = (camRot.x <= 280f && camRot.x >= 270f && deltaMouseY > 0.001f);

        if (inNormalRange || inLowerRange | inRaiseRange)      
            camRot.x += deltaMouseY * vertRotSpeed;                
        camRot.z = 0;
        cam.transform.eulerAngles = camRot;

        if (tryingToJump && isGrounded())
        {
            //prevents double jumps
            if (inJumpCooldown){
                if (jumpCooldown > 0){
                    jumpCooldown--;
                }else{
                    jumpCooldown = maxJumpCooldown;
                    inJumpCooldown = false;
                }

                return;
            }

            rigidbody.velocity += new Vector3(0, jumpStrength, 0);
            tryingToJump = false;
            inJumpCooldown = true;
            currentMovementState = movementState.AIR;
        }
    }   
}
