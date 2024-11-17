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
using UnityEngine.UI;

public class Player : Character
{
    //singleton obj that's accessible to all objects
    public static Player player;

    private Vector2 lastMoveInput = Vector2.zero;
    private Vector3 maxSpeed = new Vector3(10f, 10f, 10f);
    public float acceleration = 10f;
    private float moveAccel = 0.4f;

    private bool tryingToJump;
    private bool inJumpCooldown = false;    
    public float jumpStrength = 7f;
    [SerializeField] private float gravityAccel = -13f;
    private int jumpCooldown;
    private int maxJumpCooldown;

    private bool inLassoLock = false;
    private int lassoLockCooldown;
    private int maxLassoLockCooldown;    
    private float lassoForceMultiplier = 1.3f;

    // these need to be static so the values persist when scene reloads
    public static int roomNum;    
    public static Vector3 respawnPos;
    public static bool hasCheckpoint = false;

    [SerializeField] private float slideVel = -0.5f;
    // the max y velocity the player can have and still stick to a wall
    [SerializeField] private float wallSlideThreshold = 2f;
    private bool lockedToWall = false;
    private int wallJumpsLeft = 1;
    private int maxWallJumps = 1;

    [SerializeField] private float timeSinceJump = 0f;
    [SerializeField] private float perfectJumpWindow = 0.15f;
    private bool kickStarted = false;
    private bool kickLerping = false;
    private float yRotNormal;
    // the higher this value, the sooner the player regains camera control after the kick
    [SerializeField] private float kickStopThreshold = 8f;
    // from 0 to 1, how fast camera rotates horizontally during kick. increasing this lets player regains camera control sooner
    [SerializeField] private float kickLerpSpeed = 0.1f;

    private Camera cam;
    private GameObject lasso;

    [SerializeField] private AudioSource gunSfx;
    [SerializeField] private AudioSource lassoSfx;
    [SerializeField] private AudioSource perfectWallJumpSfx;
    [SerializeField] private AudioSource gunReloadSfx;
    [SerializeField] private AudioSource wallSlideSfx;

    private bool reloadPlayed = false;

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
        SLIDING
    };

    public movementState currentMovementState;

    void Start()
    {
        // when player dies and scene reloads
        // when more scenes are used, if scene loading is different from current scene, set hasCheckpoint to false
        if (hasCheckpoint)
            transform.position = respawnPos;

        roomNum = 1;

        // override default gravity (-9.81) to desired gravity
        Physics.gravity = new Vector3(0, gravityAccel, 0);

        // this makes the cursor stay insivible in the editor
        // to make cursor visible, press Escape  
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        maxShootCooldown = 0.5f;
        shootCooldown = maxShootCooldown;        

        cam = Camera.main;
        // lasso should be the second child of Camera for this to work
        lasso = transform.GetChild(0).GetChild(1).gameObject;        
        rigidbody = GetComponent<Rigidbody>();   

        jumpCooldown = maxJumpCooldown = 2;
        lassoLockCooldown = maxLassoLockCooldown = 5;
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

        //Debug.Log("shot enemy");
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
                if (objAimed.GetComponent<Enemy>() != null && shootCooldown >= maxShootCooldown)
                {
                    Shoot(objAimed);
                    shootCooldown = 0f;
                    reloadPlayed = false;
                }
            }
            catch
            {
                //Debug.Log("Did not shoot at anything!");
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
                lassoLockCooldown = maxLassoLockCooldown;
                inLassoLock = true;
                lassoSfx.Play();
            }
        }
        else if (context.canceled)
        {
            //Debug.Log("lasso is being canceled");
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
        bool hitFeet, hitFloor, hitWall, hitJacobsWall, gotTiming;
        for (int i = 0; i < collision.contactCount; i++)
        {
            hitFeet = collision.GetContact(i).otherCollider.bounds.max.y < playerFeetPosition();
            hitFloor = collision.GetContact(i).otherCollider.gameObject.tag == "FLOOR";
            // add jacobWall tag here when that gets in
            hitWall = collision.GetContact(i).otherCollider.gameObject.tag == "WALL";
            hitJacobsWall = collision.GetContact(i).otherCollider.gameObject.tag == "WALL" && 
                collision.GetContact(i).otherCollider.gameObject.name.Contains("Model");
            gotTiming = timeSinceJump > 0f && timeSinceJump < perfectJumpWindow;
            
            // perfect wall jump counts towards wall jump counter            
            if (hitWall && gotTiming && !hitFeet && wallJumpsLeft > 0 && !isGrounded())
            {
                Debug.Log("got kick in enter");
                kickStarted = true;                
                wallJumpsLeft--;                
                Vector3 curRot = transform.eulerAngles;                
                float wallRotY = collision.GetContact(i).otherCollider.transform.eulerAngles.y;
                
                if (hitJacobsWall)
                {
                    wallRotY += 90;
                    if(wallRotY >= 360)
                        wallRotY -= 180;
                }
                                              
                yRotNormal = curRot.y + 2 * (wallRotY + 90 - curRot.y);
                // account for if y normal being out of bounds for eulerAngles
                if (yRotNormal < 0f)
                    yRotNormal += 360f;

                if (yRotNormal >= 360f)
                    yRotNormal -= 360f;
                return;
            }
            // ignore wall collision during kick and kick lerp
            if (kickLerping)
                return;
                                   
            // allow jumping on top of walls. this takes precedence over wall jump checking
            if (hitFeet && (hitFloor || hitWall))
            {                                
                currentMovementState = movementState.GROUND;
                lockedToWall = false;                
                wallJumpsLeft = maxWallJumps;
                break;
            }
            if (hitWall && rigidbody.velocity.y < wallSlideThreshold && !isGrounded() && wallJumpsLeft > 0)
            {                
                currentMovementState = movementState.SLIDING;
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

            // this causes standing on wall issue. should be fixed when colliders are realigned
            if (hitFeet && (touchingFloor || touchingWall))
            {
                currentMovementState = movementState.GROUND;
                lockedToWall = false;                
                wallJumpsLeft = maxWallJumps;
                break;
            }
            else if (touchingWall && rigidbody.velocity.y < wallSlideThreshold && !isGrounded() && wallJumpsLeft > 0)
            {                
                currentMovementState = movementState.SLIDING;
                lockedToWall = true;
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
        return (currentMovementState == movementState.SLIDING);
    }

    public int GetHealth()
    {
        return health;
    }

    protected override void Death()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // reset floorsInitialized so floors can reset offmeshlinks
        Floor.floorsInitialized = 0;
        Enemy.enemiesInitialized = 0;
        Enemy.enemiesInRoom = 0;
    }

    public override void TakeDamage(int damage)
    {
        health -= damage;
        // this should get the hearts in the hierarchy order so you don't need to sort
        Image[] images = FindObjectsOfType<Image>();
        List<Image> hearts = new List<Image>();
        foreach (Image img in images)
        {
            if (img.gameObject.name.Contains("Heart"))
                hearts.Add(img);
        }
        Destroy(hearts[hearts.Count - 1].gameObject);

        if (health <= 0)
            Death();
    }

    //courtesy of internet physics/game dev guru. Calculates force needed to launch player towards hook
    //if needed, we can tweak this so that distance is not a factor or is less of a factor
    public Vector3 calculateLassoForce(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) 
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        Vector3 velocity = velocityY + velocityXZ;

        //Debug.Log(velocity);
        return velocity;
    }

    public void lassoLaunch(Vector3 targetPosition, float height)
    {
        rigidbody.velocity = calculateLassoForce(transform.position, targetPosition, height) * lassoForceMultiplier;
    }

    public float playerFeetPosition()
    {
        return GetComponent<BoxCollider>().bounds.min.y + 0.05f;
    }

    void FixedUpdate()
    {
        Debug.Log("State: " + currentMovementState);        

        //forces camera to look straight as you're opening up scene
        if (Time.timeSinceLevelLoad < 0.1f)
            return;        

        //guarantees lasso state won't be overwritten
        if (inLassoLock)
        {
            currentMovementState = movementState.SWINGING;

            if (lassoLockCooldown > 0)
            {
                lassoLockCooldown--;
            }
            else
            {
                lassoLockCooldown = maxLassoLockCooldown;
                inLassoLock = false;
            }
        }       

        if (currentMovementState == movementState.SLIDING)
        {
            if(wallSlideSfx != null && !wallSlideSfx.isPlaying)
                wallSlideSfx.Play();
            rigidbody.velocity = new Vector3(0, slideVel, 0);
        }
        else if (currentMovementState == movementState.SWINGING)
        {
            //You cannot move normally when you're swinging.
            lastMoveInput = Vector2.zero;
            //return;
            /*
            // need to assign y velocity first so it is not overriden
            Vector3 newVel = new Vector3(0, rigidbody.velocity.y, 0);


            newVel += (transform.right * lastMoveInput.x +
                       transform.forward * lastMoveInput.y) * speed;

            rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, newVel, moveAccel);
            */
        }
        else
        {
            // need to assign y velocity first so it is not overriden
            Vector3 newVel = new Vector3(0, rigidbody.velocity.y, 0);
            newVel += (transform.right * lastMoveInput.x +
                       transform.forward * lastMoveInput.y) * speed;

            rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, newVel, moveAccel);
        }

        // Input.GetAxis is the change in value since last frame        
        float deltaMouseX = Input.GetAxis("Mouse X");
        float deltaMouseY = Input.GetAxis("Mouse Y");
                     
        // kick rotation momentarily overrides normal rotation
        // consider using a time variable to unstuck for emergencies
        if (kickLerping)
        {            
            Vector3 newRot = transform.eulerAngles;
            // LerpAngle handles the wrap around from 360 -> 0 degrees
            newRot.y = Mathf.LerpAngle(newRot.y, yRotNormal, kickLerpSpeed);
            transform.eulerAngles = newRot;            

            // what about 360?
            //if(transform.eulerAngles.y > )
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

            rigidbody.velocity += new Vector3(0, 1.2f * jumpStrength, 0);

            kickStarted = false;
            tryingToJump = false;
            kickLerping = true;
            timeSinceJump = 0f;
        }
        else if (tryingToJump && isGrounded())
        {            
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
            wallJumpsLeft--;
            if (wallSlideSfx != null && wallSlideSfx.isPlaying)
                wallSlideSfx.Stop();
        }
        else if (tryingToJump)
        {
            timeSinceJump += Time.deltaTime;
        }        

        shootCooldown += Time.deltaTime;

        if(shootCooldown > maxShootCooldown && !reloadPlayed && gunReloadSfx != null)
        {
            gunReloadSfx.Play();
            reloadPlayed = true;
        }
    }   
}
