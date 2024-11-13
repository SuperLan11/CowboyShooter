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

    [SerializeField] private float slideSpeed = -0.5f;
    [SerializeField] private float wallSlideThreshold = 2f;
    private bool lockedToWall = false;
    
    private float timeSinceJump = 0f;
    private float perfectJumpWindow = 0.15f;
    private bool kickStarted = false;
    private bool kickLerping = false;
    private float yRotNormal;
    // the higher this value, the sooner the player regains camera control after the kick
    private float kickStopThreshold = 5f;
    // from 0 to 1, how fast camera rotates horizontally during kick
    private float kickLerpSpeed = 0.1f;

    private Camera cam;
    private GameObject lasso;

    [SerializeField] private AudioSource gunSfx;
    [SerializeField] private AudioSource lassoSfx;
    [SerializeField] private AudioSource perfectWallJumpSfx;

    // IMPORTANT! If you assign these values here, they must be the same as the inspector (i think)
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
   
    protected override void Shoot(GameObject enemy)
    {
        if (gunSfx != null)
            gunSfx.Play();

        enemy.GetComponent<Enemy>().TakeDamage(1);
    }

    public GameObject ObjAimedAt()
    {
        RaycastHit hit;
        // use cam forward instead of player since player can't rotate vertically
        if (Physics.Raycast(transform.position, cam.transform.forward, out hit, Mathf.Infinity))
        {                    
            return hit.transform.gameObject;
        }
        return null;
    }

    public void ShootActivated(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            try
            {
                GameObject objAimed = ObjAimedAt();
                //Debug.Log("objAimed: " + objAimed.name);
                if (objAimed.GetComponent<Enemy>() != null)
                    Shoot(objAimed);
            }
            catch
            {
                Debug.Log("Did not shoot at anything!");
            }
        }
    }

    public void LassoActivated(InputAction.CallbackContext context)
    {        
        if (context.started)
        {
            bool valid = lasso.GetComponent<Lasso>().StartLasso();

            if (valid)
            {
                player.currentMovementState = movementState.SWINGING;
                lassoSfx.Play();
            }
        }
        else if (context.canceled)
        {
            //prevents player from being in air state after just tapping RMB
            if (player.currentMovementState != movementState.GROUND)
            {
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
            timeSinceJump = 0f;
        }
        else if (context.canceled)
        {
            tryingToJump = false;
        }
    }

    
    private void OnCollisionEnter(Collision collision)
    {
        bool hitFeet, onFloor, hitWall;
        for (int i = 0; i < collision.contactCount; i++)
        {
            hitFeet = collision.GetContact(i).otherCollider.bounds.max.y < GetComponent<BoxCollider>().bounds.min.y + 0.05f;
            onFloor = collision.GetContact(i).otherCollider.gameObject.tag == "FLOOR";
            hitWall = collision.GetContact(i).otherCollider.gameObject.tag == "WALL";

            //Debug.Log("time since jump: " + timeSinceJump);
            // ignore wall collision during kick and kick lerp
            if (hitWall && timeSinceJump > 0f && timeSinceJump < perfectJumpWindow)
            {
                kickStarted = true;
                return;
            }           
            if (kickLerping)
                return;
                                   
            // allow jumping on top of walls. this takes precedence over wall jump checking
            if (hitFeet && (onFloor || hitWall))
            {
                Debug.Log("set to GROUND state");
                lockedToWall = false;
                currentMovementState = movementState.GROUND;
                // without break, movement state is determined by last contact
                break;
            }
            if (hitWall && rigidbody.velocity.y < wallSlideThreshold && !isGrounded())
            {
                Debug.Log("set to WALL state");
                currentMovementState = movementState.WALL;
                lockedToWall = true;
                break;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            bool touchingWall = collision.GetContact(i).otherCollider.gameObject.tag == "WALL";
            bool touchingFloor = collision.GetContact(i).otherCollider.gameObject.tag == "FLOOR";
            bool hitFeet = collision.collider.bounds.max.y < GetComponent<BoxCollider>().bounds.min.y + 0.01f;

            if (touchingFloor || (touchingWall && hitFeet))
            {
                currentMovementState = movementState.GROUND;
                lockedToWall = false;
                break;
            }
            else if (touchingWall && rigidbody.velocity.y < wallSlideThreshold && !isGrounded())
            {
                currentMovementState = movementState.WALL;
                lockedToWall = true;
                rigidbody.velocity = new Vector3(0, slideSpeed, 0);
                break;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // if you slide off a wall or jump over a wall, return to air state        
        bool isWall = collision.gameObject.tag == "WALL";
        bool isFloor = collision.gameObject.tag == "FLOOR";
        bool standingOnWall = isWall && GetComponent<BoxCollider>().bounds.min.y + 0.1f > collision.gameObject.GetComponent<MeshRenderer>().bounds.max.y;

        if (isFloor || standingOnWall)
            currentMovementState = movementState.AIR;
        else if (isWall && !isGrounded() && !lockedToWall)        
            currentMovementState = movementState.AIR;        
    } 

    public bool isGrounded()
    {
        return (currentMovementState == movementState.GROUND);
    }

    public bool isOnWall()
    {
        return (currentMovementState == movementState.WALL);
    }

    protected override void Death()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // reset floorsInitialized so floors can reset offmeshlinks
        Floor.floorsInitialized = 0;
    }

    void FixedUpdate()
    {
        //Debug.Log("State: " + currentMovementState);        

        //forces camera to look straight as you're opening up scene
        if (Time.timeSinceLevelLoad < 0.1f)
            return;               

        if (currentMovementState == movementState.WALL)
        {            
            rigidbody.velocity = new Vector3(0, slideSpeed, 0);            
        }
        else
        {
            // need to assign y velocity first so it is not overriden
            rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0);

            // moves player left/right/forward/backward from direction they're facing
            rigidbody.velocity += (transform.right * lastMoveInput.x +
                                 transform.forward * lastMoveInput.y) * speed;
        }

        // Input.GetAxis is the change in value since last frame        
        float deltaMouseX = Input.GetAxis("Mouse X");
        float deltaMouseY = Input.GetAxis("Mouse Y");
                     
        // kick rotation momentarily overrides normal rotation
        if (kickLerping)
        {            
            Vector3 newRot = transform.eulerAngles;            
            newRot.y = yRotNormal;            
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, newRot, kickLerpSpeed);

            if (yRotNormal < transform.eulerAngles.y && transform.eulerAngles.y < yRotNormal + kickStopThreshold)            
                kickLerping = false;
            else if (yRotNormal > transform.eulerAngles.y && transform.eulerAngles.y > yRotNormal - kickStopThreshold)            
                kickLerping = false;            
        }
        else
        {
            // Player will not scroll vertically so that transform.forward doesn't move into the sky
            Vector3 playerRot = transform.rotation.eulerAngles;
            playerRot.y += deltaMouseX * horRotSpeed;
            // just in case
            playerRot.x = 0;
            playerRot.z = 0;
            transform.eulerAngles = playerRot;

            // The camera only scrolls vertically since the player parent object handles horizontal scroll
            Vector3 camRot = cam.transform.rotation.eulerAngles;

            // camRot.x starts decreasing from 360 when you look up and is positive downwards        
            bool inNormalRange = (camRot.x > 280f || camRot.x < 80f);
            bool inLowerRange = (camRot.x <= 280f && camRot.x >= 270f && deltaMouseY < -0.001f);
            bool inRaiseRange = (camRot.x >= 80f && camRot.x <= 90f && deltaMouseY > 0.001f);

            // -= because xRot is negative upwards
            if (inNormalRange || inLowerRange | inRaiseRange)
                camRot.x -= deltaMouseY * vertRotSpeed;
            camRot.z = 0;
            cam.transform.eulerAngles = camRot;
        }

        if (kickStarted)
        {
            perfectWallJumpSfx.Play();
            Debug.Log("kicking");

            Vector3 curRot = transform.eulerAngles;           
            yRotNormal = curRot.y + 2*(90 - curRot.y);
            
            // in case of negative rotation
            if (yRotNormal < 0)
                yRotNormal += 360f;
            //Debug.Log("normal of " + curRot.y + " is " + yRotNormal);

            rigidbody.velocity += new Vector3(0, 1.2f * jumpStrength, 0);                                    

            kickStarted = false;
            tryingToJump = false;
            kickLerping = true;
            timeSinceJump = 0f;            
        }
        else if (tryingToJump && isGrounded())
        {
            //Debug.Log("trying to jump");
            //prevents double jumps
            if (inJumpCooldown)
            {
                if (jumpCooldown > 0)
                {
                    jumpCooldown--;
                }
                else
                {
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
        else if (tryingToJump && isOnWall())
        {
            lockedToWall = false;
            currentMovementState = movementState.AIR;
            rigidbody.velocity += new Vector3(0, jumpStrength, 0);
        }
        else if (tryingToJump)
        {
            timeSinceJump += Time.deltaTime;
        }         
    }   
}
