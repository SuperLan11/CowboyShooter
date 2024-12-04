/*
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
using Quaternion = UnityEngine.Quaternion;
using UnityEngine.UI;
using System.Runtime.ConstrainedExecution;
//using UnityEditor.Rendering.LookDev;

public class Player : Character
{
    //singleton obj that's accessible to all objects
    public static Player player;

    [SerializeField] protected float speed;
    private Vector2 lastMoveInput = Vector2.zero;
    //!Must be between 0 and 1f. Determines how fast player accelerates towards max speed
    private float moveAccel = 0.4f;
    
    private float healthLastFrame;

    private bool tryingToJump;
    private bool holdingRMB;
    private bool holdingRestart;
    private bool inJumpCooldown = false;
    public float jumpStrength = 7f;
    [SerializeField] private float gravityAccel = -13f;
    private int jumpCooldown;
    private int maxJumpCooldown;

    [SerializeField] public float maxShootCooldown;
    [System.NonSerialized] public float shootCooldown;

    private int maxRestartCooldown;
    private int restartCooldown;

    private bool inLassoLock = false;
    private bool inLassoCooldown = false;
    private int lassoLockCooldown;
    private int maxLassoLockCooldown;    
    private int lassoCooldown;
    private int maxLassoCooldown;
    private const float startingLassoForceMultiplier = 10f;  //originally 15f
    private const float lassoForceIncrease = 0.15f;
    private float maxLassoSpeed = 30f;
    private float lassoForceMultiplier;
    [System.NonSerialized] public float maxLassoRange = 25f;

    // these need to be static so the values persist when scene reloads    
    public static Vector3 respawnPos;
    public static Vector3 respawnRot;
    public static bool hasCheckpoint = false;
    public List<Enemy> enemiesInRoom = new List<Enemy>();

    [SerializeField] private float slideVel = -0.5f;
    // the max y velocity the player can have and still stick to a wall
    [SerializeField] private float wallSlideThreshold = 2f;
    private bool lockedToWall = false;
    private int wallJumpsLeft = 1;
    private int maxWallJumps = 1;

    private float timeSinceJump = 0f;
    [SerializeField] private float perfectJumpWindow = 0.15f;
    private bool kickStarted = false;
    private bool kickLerping = false;
    private float yRotNormal;
    // the higher this value, the sooner the player regains camera control after the kick
    [SerializeField] private float kickStopThreshold = 8f;
    // from 0 to 1, how fast camera rotates horizontally during kick. increasing this lets player regains camera control sooner
    [SerializeField] private float kickLerpSpeed = 0.1f;

    private const float KNOCKBACK_FORCE = 2.5f;

    private Camera cam;
    private GameObject lasso;

    [SerializeField] private AudioSource gunSfx;
    [SerializeField] private AudioSource lassoSfx;
    [SerializeField] private AudioSource perfectWallJumpSfx;
    [SerializeField] private AudioSource gunReloadSfx;
    [SerializeField] private AudioSource wallSlideSfx;    
    [SerializeField] private AudioSource takeDamageSfx;
    [SerializeField] public AudioSource yeehawSfx;

    [SerializeField] private GameObject healthBar;
    [SerializeField] private MeshRenderer lassoMesh;
    [SerializeField] private ParticleSystem shootingSystem;
    //[SerializeField] private ParticleSystem impactParticleSystem;
    [SerializeField] private TrailRenderer bulletTrail;
    [SerializeField] private Transform gunTip;


    private bool reloadPlayed = false;

    // Snap is how closely the camera stays to the actual mouse value
    // Sensitivity changes how much magnitude moving the mouse has
    [SerializeField] private float horCamSnap = 10f;
    [SerializeField] private float vertCamSnap = 10f;

    //these values are used in so many places that I think they need to not be serialized. Too many chances for the values
    //to get screwed up with the object in the scene

    private float mouseSensitivityX = 0.7f;
    private float mouseSensitivityY = 1f;
    private float curMouseX = 0f;
    private float curMouseY = 0f;
    
    [SerializeField] private float camLockDist = 50f;    

    public enum movementState
    {
        GROUND,
        AIR,
        SWINGING,
        HANGING,
        SLIDING,
        FLYING,
    };

    [System.NonSerialized] public movementState currentMovementState;

    void Start()
    {
        DeathMenu.deathMenuActive = false;
        //GameManager.gameManager.CleanupScene();
        transform.rotation = Quaternion.Normalize(transform.rotation);        

        //variables need to be changed to account for 3rd level
        if (SceneManager.GetActiveScene().buildIndex == 3){
            maxLassoRange *= 3f;
            speed *= 2f;
            maxLassoSpeed *= 1.5f;
        }
        
        if (gunSfx == null)
        {
            Debug.LogError("You ain't got a gun sound, partner!");
        }

        // when player dies and scene reloads
        // when more scenes are used, if scene loading is different from current scene, set hasCheckpoint to false
        if (hasCheckpoint)
        {
            transform.position = respawnPos;
            transform.eulerAngles = respawnRot;
        }

        currentMovementState = movementState.GROUND;

        // Important: this affects gravity for everything!
        Physics.gravity = new Vector3(0, gravityAccel, 0);

        // this makes the cursor stay insivible in the editor
        // to make cursor visible, press Escape  
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        lassoForceMultiplier = startingLassoForceMultiplier;
        
        maxShootCooldown = 0.5f;
        shootCooldown = maxShootCooldown;  

        jumpCooldown = maxJumpCooldown = 2;

        lassoCooldown = maxLassoCooldown = 22;
        lassoLockCooldown = maxLassoLockCooldown = 5;
        
        //remember it's not in terms of frames, so a value of 60 does not mean it'll wait 1 second.
        restartCooldown = maxRestartCooldown = 40;      

        cam = Camera.main;
        // lasso should be the second child of Camera for this to work
        lasso = transform.GetChild(0).GetChild(1).gameObject;        
        rigidbody = GetComponent<Rigidbody>();           

        holdingRMB = false;
        holdingRestart = false;

        SetPlayerMouseSensitivity();

        healthLastFrame = health;
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
   
    private void Shoot(GameObject enemy)
    {
        //Debug.Log("shot enemy");
        enemy.GetComponent<Enemy>().TakeDamage(1);
        enemy.GetComponent<Enemy>().SetGotShot(true);
    }

    private void ShootExplodingBarrel(GameObject explodingBarrel)
    {
        explodingBarrel.GetComponent<ExplodingBarrel>().Explode();
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit hit){
        float time = 0;
        Vector3 startPosition = trail.transform.position;

        while (time < 1)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hit.point, time);
            time += (Time.deltaTime / trail.time);

            yield return null;
        }

        trail.transform.position = hit.point;
        //Instantiate(impactParticleSystem, hit.point, Quaternion.LookRotation(hit.normal));

        Destroy(trail.gameObject, trail.time);
    }

    //!Returns the name of the GameObject, NOT the name of the tag!
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
        //without second condition, player can shoot once while being in pause menu
        if (context.started && !PauseMenu.gameIsPaused)
        { 
            try
            {
                GameObject objAimed = ObjAimedAt();
                //Debug.Log("objAimed: " + objAimed.name);
                if (shootCooldown >= maxShootCooldown)
                {                    
                    gunSfx.Play();
                    shootCooldown = 0f;
                    reloadPlayed = false;

                    //gun trail effect
                    if (Physics.Raycast(gunTip.position, gunTip.transform.forward, out RaycastHit hit, Mathf.Infinity))
                    {                    
                        TrailRenderer trail = Instantiate(bulletTrail, gunTip.position, Quaternion.identity);
                        StartCoroutine(SpawnTrail(trail, hit));
                    }

                    if(objAimed.GetComponent<Enemy>() != null)
                    {
                        Shoot(objAimed);

                        //knockback
                        Vector3 enemyDirection = (objAimed.transform.position - this.transform.position).normalized;
                        Vector3 knockback = enemyDirection * KNOCKBACK_FORCE;
                        knockback.y = 0f;
                        objAimed.GetComponent<Enemy>().ApplyForce(knockback);
                    }
                    else if (objAimed.GetComponentInChildren<ExplodingBarrel>() != null)
                    {
                        ShootExplodingBarrel(objAimed);
                    }
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

            if (valid && !inLassoCooldown)
            {
                player.currentMovementState = movementState.SWINGING;
                lassoLockCooldown = maxLassoLockCooldown;
                inLassoLock = true;
                inLassoCooldown = true;
                lassoSfx.Play();
            }
        }
        else if (context.performed)
        {
            //actual logic for holding RMB must go somewhere else since context.performed is only called the FIRST frame the user holds RMB
            holdingRMB = true;
        }
        else if (context.canceled)
        {
            holdingRMB = false;
            lassoForceMultiplier = startingLassoForceMultiplier;
            
            //prevents player from being in air state after just tapping RMB
            if (!isGrounded() && player.currentMovementState == movementState.SWINGING)
            {
                player.currentMovementState = movementState.FLYING;
            }
            else if (!isGrounded() && player.currentMovementState == movementState.HANGING)
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

    public void RestartLevelActivated(InputAction.CallbackContext context)
    {
        bool firstPressingR = context.started && !holdingRestart;
        bool holdingR = context.performed;

        if (firstPressingR || holdingR)
        {
            holdingRestart = true;
        }
        else{
            holdingRestart = false;
            restartCooldown = maxRestartCooldown;
        }
    }

    public void PauseActivated(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            PauseMenu.gameIsPaused = !PauseMenu.gameIsPaused;
        }

        //when user resumes, appropriate settings must be applied
        if (!PauseMenu.gameIsPaused)
        {
            mouseSensitivityX = mouseSensitivityY = GameManager.mouseSensitivity;
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {       
        bool hitFeet, hitFloor, hitWall, hitCeiling, hitJacobsWall, hitMetalWall, gotTiming;
        for (int i = 0; i < collision.contactCount; i++)
        {
            hitFeet = collision.GetContact(i).otherCollider.bounds.max.y < playerFeetPosition();
            hitFloor = collision.GetContact(i).otherCollider.gameObject.tag == "FLOOR";
            // add jacobWall tag here when that gets in
            hitWall = collision.GetContact(i).otherCollider.gameObject.tag == "WALL";
            hitCeiling = collision.GetContact(i).otherCollider.gameObject.tag == "CEILING" && collision.GetContact(i).otherCollider.bounds.min.y > playerFeetPosition();
            hitJacobsWall = collision.GetContact(i).otherCollider.gameObject.tag == "WALL" && 
                collision.GetContact(i).otherCollider.gameObject.name.Contains("Model");
            // used MetalWalls Layer instead of a tag since it's faster than checking tag1 == tag2 and I didn't want to rework code that uses the WALL tag
            hitMetalWall = collision.GetContact(i).otherCollider.gameObject.layer == LayerMask.NameToLayer("MetalWalls");
            gotTiming = timeSinceJump > 0f && timeSinceJump < perfectJumpWindow;
            
            // perfect wall jump counts towards wall jump counter            
            if (hitWall && !hitMetalWall && gotTiming && !hitFeet && wallJumpsLeft > 0 && !isGrounded())
            {                
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


            if (hitCeiling && usingLasso())
            {
                //prevent player from getting stuck on ceiling
                rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
            }   

            // allow jumping on top of walls. this takes precedence over wall jump checking
            if (hitFeet && (hitFloor || hitWall))
            {                                
                currentMovementState = movementState.GROUND;
                lockedToWall = false;                
                wallJumpsLeft = maxWallJumps;
                break;
            }
            if (hitWall && !hitMetalWall && rigidbody.velocity.y < wallSlideThreshold && !isGrounded() && wallJumpsLeft > 0)
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
            bool touchingMetalWall = collision.GetContact(i).otherCollider.gameObject.layer == LayerMask.NameToLayer("MetalWalls");
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
            else if (touchingWall && !touchingMetalWall && rigidbody.velocity.y < wallSlideThreshold && !isGrounded() && wallJumpsLeft > 0)
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
        bool standingOnWall = isWall && GetComponent<BoxCollider>().bounds.min.y + 0.1f > collision.gameObject.GetComponent<BoxCollider>().bounds.max.y;

        if (isFloor || standingOnWall)
            currentMovementState = movementState.AIR;
        else if (isWall && !isGrounded() && !lockedToWall)
            currentMovementState = movementState.AIR;
    } 

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "HOOK" && holdingRMB)
        {
            currentMovementState = movementState.HANGING;
        }
    }

    public bool isGrounded()
    {
        return (currentMovementState == movementState.GROUND);
    }

    public bool usingLasso()
    {
        return (Player.player.currentMovementState == Player.movementState.HANGING ||
            Player.player.currentMovementState == Player.movementState.SWINGING);
    }

    public bool isOnWall()
    {
        return (currentMovementState == movementState.SLIDING);
    }

    public int GetHealth()
    {
        return health;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public bool AtFullHealth(){
        return (GetHealth() == GetMaxHealth());
    }

    public void SetHealth(int h)
    {
        health = h;
    }

    private void UpdateHealthBar()
    {
        List<GameObject> hearts = new List<GameObject>();
        for (int i = 0; i < healthBar.transform.childCount; i++)
        {
            hearts.Add(healthBar.transform.GetChild(i).gameObject);
        }

        int numActiveHearts = 0;
        for (int i = 0; i < hearts.Count; i++)
        {
            if (hearts[i].activeSelf)
            {
                numActiveHearts++;
            }
        }


        int healthDisparity = health - numActiveHearts;
        bool tooManyHearts = healthDisparity < 0;
        bool notEnoughHearts = healthDisparity > 0;

        if (tooManyHearts)
        {
            int healthIndex = hearts.Count - 1;

            while (healthDisparity < 0 && healthIndex > 0)
            {
                if (hearts[healthIndex].activeSelf)
                {
                    hearts[healthIndex].SetActive(false);
                    healthDisparity++;
                }

                healthIndex--;
            }
        }
        else if (notEnoughHearts)
        {
            int healthIndex = 0;
            
            while (healthDisparity > 0 && healthIndex < hearts.Count)
            {
                if (!hearts[healthIndex].activeSelf)
                {
                    hearts[healthIndex].SetActive(true);
                    healthDisparity--;
                }
                
                healthIndex++;
            }
        }
        else
        {
            Debug.LogError("This aint s'posed to happen, partner!");
        }
    }

    protected override void Death()
    {
        GameManager.gameManager.StoreTimerValue(Clock.rawSeconds);
        DeathMenu.deathMenuActive = true;

        
        // Enemy[] enemies = GameManager.gameManager.originalEnemyList;
        // foreach (Enemy e in enemies)
        // {
        //     if (e.checkpointNum == GameManager.currentCheckpoint)
        //     {
        //         Debug.Log("Good morning");
        //         Enemy tempEnemy = e;
        //         Destroy(e.gameObject);
        //         Instantiate(tempEnemy.gameObject, tempEnemy.spawnPos, Quaternion.identity);
        //     }
        // }

        // /*
        // foreach(Enemy e in enemiesInRoom)
        // {
        //     Instantiate(e, e.spawnPos, Quaternion.identity);
        // }
        // */

        // health = maxHealth;
        // transform.position = respawnPos;
        // transform.eulerAngles = respawnRot;

        GameManager.gameManager.RestartLevel();
        GameManager.gameManager.CleanupScene();
        
        /*foreach (Enemy enemy in enemiesInRoom)
        {
            enemy.transform.position = enemy.spawnPos;
        }
        transform.position = respawnPos;
        transform.eulerAngles = respawnRot;*/
    }

    public override void TakeDamage(int damage)
    {
        health -= damage;
        if(takeDamageSfx != null)
            takeDamageSfx.Play();

        SetHealth(health);
        if (health == 0)
            Death();
    }    

    //courtesy of internet physics/game dev guru. Calculates force needed to launch player towards hook
    //most of what this function does is probably unnecessary, but we should only change it if needed cause it works!
    public Vector3 calculateLassoForce(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) 
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        Vector3 velocity = velocityY + velocityXZ;

        return velocity.normalized;
    }

    public void lassoLaunch(Vector3 targetPosition, float height)
    {
        float oldVelocityMagnitude = rigidbody.velocity.magnitude;
        
        Vector3 initialLassoForce = calculateLassoForce(transform.position, targetPosition, height) * lassoForceMultiplier;
        rigidbody.velocity = (initialLassoForce.magnitude <= maxLassoSpeed ? initialLassoForce : initialLassoForce.normalized * maxLassoSpeed);

        //makes sure you keep your momentum if you were going fast already
        if (rigidbody.velocity.magnitude < oldVelocityMagnitude)
        {
            rigidbody.velocity = rigidbody.velocity.normalized * oldVelocityMagnitude;
        }
        Debug.Log(rigidbody.velocity.magnitude);
    }

    public float playerFeetPosition()
    {
        return GetComponent<BoxCollider>().bounds.min.y + 0.05f;
    }

    private void CheckHookLock()
    {        
        GameObject[] hooks = GameObject.FindGameObjectsWithTag("HOOK");
        foreach(GameObject hookObj in hooks)
        {
            Vector2 hook2DPos = Camera.main.WorldToScreenPoint(hookObj.transform.position);
            float distToHook = Vector2.Distance(Input.mousePosition, hook2DPos);            
            if (distToHook < camLockDist)
            {                
                RaycastHit hit;
                
                Vector3 hookDir = (hookObj.transform.position - cam.transform.position).normalized;
                float FOV = cam.fieldOfView;
                float dotThreshold = Mathf.Cos(Mathf.Deg2Rad * (FOV / 2f));
                float dotProduct = Vector3.Dot(hookDir, cam.transform.forward);
                bool inGeneralForwardDirection = (dotProduct < dotThreshold);

                Vector3 screenPoint = cam.WorldToViewportPoint(hookObj.transform.position);
                bool offScreen = !(screenPoint.z > 0 && screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1);
                
                
                if (!inGeneralForwardDirection || offScreen)
                {
                    return;
                }

                Physics.Raycast(cam.transform.position, hookDir, out hit, Mathf.Infinity);                
                if (hit.transform.gameObject.tag == "HOOK")
                {
                    // transform.LookAt ignores rigidbody constraints, which can cause movement bugs
                    // only rotate horizontally for player and vertically for camera to avoid this
                    Vector3 hookHorPos = new Vector3(hookObj.transform.position.x, transform.position.y + transform.forward.y, hookObj.transform.position.z);                                                            
                    player.transform.LookAt(hookHorPos);
                    cam.transform.LookAt(hookObj.transform.position);
                    return;
                }
            }                                                                                          
        }        
    }

    private void CheckEnemyLock()
    {        
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach(Enemy enemy in enemies)
        {
            if (GameManager.currentCheckpoint != enemy.checkpointNum)
                continue;

            Vector2 enemy2DPos = Camera.main.WorldToScreenPoint(enemy.transform.position);
            float distToEnemy = Vector2.Distance(Input.mousePosition, enemy2DPos);            
            if (distToEnemy < camLockDist)
            {                
                RaycastHit hit;
                Vector3 enemyDir = (enemy.transform.position - cam.transform.position).normalized;
                Physics.Raycast(cam.transform.position, enemyDir, out hit, Mathf.Infinity);                
                if (hit.transform.gameObject.GetComponent<Enemy>() != null)
                {
                    // transform.LookAt ignores rigidbody constraints, which can cause movement bugs
                    // only rotate horizontally for player and vertically for camera to avoid this
                    Vector3 enemyHorPos = new Vector3(enemy.transform.position.x, transform.position.y + transform.forward.y, enemy.transform.position.z);                                                            
                    player.transform.LookAt(enemyHorPos);
                    cam.transform.LookAt(enemy.transform.position);
                    return;
                }
            }                                                                                          
        }        
    }

    public void SetPlayerMouseSensitivity()
    {
        mouseSensitivityX = mouseSensitivityY = GameManager.mouseSensitivity;
    }

    //Dragon's Den of the Movement Code
    void FixedUpdate()
    {
        //Debug.Log("State: " + currentMovementState);
        //Debug.Log(rigidbody.velocity.magnitude);        

        //forces camera to look straight as you're opening up scene        
        if (Time.timeSinceLevelLoad < 0.1f)
            return;

        if (health != healthLastFrame)
        {
            UpdateHealthBar();
            //Debug.Log("this is being called");
        }

        lassoMesh.enabled = !usingLasso();        

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

        if (inLassoCooldown)
        {
            if (lassoCooldown > 0)
            {
                lassoCooldown--;
            }
            else
            {
                lassoCooldown = maxLassoCooldown;
                inLassoCooldown = false;
            }
        }

        if (holdingRestart)
        {
            if (restartCooldown > 0)
            {
                restartCooldown--;
            }
            else
            {
                GameManager.gameManager.RestartLevel();
                GameManager.gameManager.CleanupScene();
            }
        }



        if (currentMovementState == movementState.SWINGING)
        {
            if (holdingRMB)
            {
                lassoForceMultiplier += lassoForceIncrease;
                lasso.GetComponent<Lasso>().ContinueLasso();
            }
        }
        else if (currentMovementState == movementState.SLIDING)
        {
            if (wallSlideSfx != null && !wallSlideSfx.isPlaying)
                wallSlideSfx.Play();
            rigidbody.velocity = new Vector3(0, slideVel, 0);
        }
        else if (currentMovementState == movementState.HANGING)
        {
            int hangModifier = 5;
            rigidbody.velocity = new Vector3(0, gravityAccel / hangModifier, 0);
        }
        else if (currentMovementState == movementState.GROUND || currentMovementState == movementState.AIR)
        {
            //!Acceleration based movement. The only things you should change if you want to change movement are
            //!speed and moveAccel 

            // need to assign y velocity first so it is not overriden           
            //Debug.Log("transform.forward: " + transform.forward);
            Vector3 newVel = new Vector3(0, rigidbody.velocity.y, 0);
            newVel += (transform.right * lastMoveInput.x +
                       transform.forward * lastMoveInput.y) * speed;

            rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, newVel, moveAccel);
        }
        else if (currentMovementState == movementState.FLYING)
        {            
            //aim assist
            //CheckHookLock();
            //CheckEnemyLock();            
        }

        // Input.GetAxis is the change in value since last frame                
        curMouseX = Mathf.Lerp(curMouseX, Input.GetAxis("Mouse X"), horCamSnap * Time.deltaTime);
        curMouseY = Mathf.Lerp(curMouseY, Input.GetAxis("Mouse Y"), vertCamSnap * Time.deltaTime);

        // kick rotation momentarily overrides normal rotation
        // consider using a time variable to unstuck for emergencies
        if (kickLerping)
        {            
            Vector3 newRot = transform.eulerAngles;
            // LerpAngle handles the wrap around from 360 -> 0 degrees
            newRot.y = Mathf.LerpAngle(newRot.y, yRotNormal, kickLerpSpeed);
            transform.eulerAngles = newRot;            
            
            if (yRotNormal < transform.eulerAngles.y && transform.eulerAngles.y < yRotNormal + kickStopThreshold)            
                kickLerping = false;
            else if (yRotNormal > transform.eulerAngles.y && transform.eulerAngles.y > yRotNormal - kickStopThreshold)            
                kickLerping = false;            
        }
        else
        {
            float mouseX = curMouseX * mouseSensitivityX * 100f * Time.deltaTime;
            float mouseY = curMouseY * mouseSensitivityY * 100f * Time.deltaTime;
            //float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY * 100f * Time.deltaTime;

            // Player will not scroll vertically so that transform.forward doesn't move into the sky
            Vector3 playerRot = transform.eulerAngles;
            Vector3 newPlayerRot = Vector3.zero;
            newPlayerRot.y = playerRot.y + mouseX;
            // just in case
            newPlayerRot.x = playerRot.x;
            newPlayerRot.z = 0;
            transform.eulerAngles = Vector3.Lerp(playerRot, newPlayerRot, horCamSnap);
            //transform.eulerAngles = Quaternion.Lerp(playerRot, newPlayerRot, horCamSnap);

            // The camera only scrolls vertically since the player parent object handles horizontal scroll            
            Vector3 camRot = cam.transform.eulerAngles;
            Vector3 newCamRot = Vector3.zero;
            
            float deltaMouseY = Input.GetAxis("Mouse Y");
            //Debug.Log("mouseY: " + mouseY);

            // camRot.x starts decreasing from 360 when you look up and is positive downwards   
            bool inNormalRange = (camRot.x > 280f || camRot.x < 80f);
            bool inLowerRange = (camRot.x <= 280f && camRot.x >= 270f && deltaMouseY < -0.001f);
            bool inRaiseRange = (camRot.x >= 80f && camRot.x <= 90f && deltaMouseY > 0.001f);

            newCamRot.z = 0;
            newCamRot.y = newPlayerRot.y;
            if (inNormalRange || inLowerRange | inRaiseRange)
            {
                // - because xRot is negative upwards                
                newCamRot.x = camRot.x - mouseY;                
                cam.transform.eulerAngles = Vector3.Lerp(camRot, newCamRot, vertCamSnap);
            }
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

        healthLastFrame = health;
    }   
}
