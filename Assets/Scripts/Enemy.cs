/*
@Authors - Patrick, Landon
@Description - Enemy class. Different enemy prefabs should be able to use the same script
*/

using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

public abstract class Enemy : Character
{
    protected NavMeshAgent agent;

    // make these Transform since Vector3 can't be dragged in inspector
    [SerializeField] protected Transform destination1;
    [SerializeField] protected Transform destination2;
    protected List<Vector3> destList = new List<Vector3>();
    // assuming all enemies can jump the same distance
    [SerializeField] public static float maxJumpDist = 7f;
    protected float speed;

    public static int enemiesInitialized = 0;
    public static int enemiesInRoom = 0;

    // attackCooldown var not needed as coroutines are used
    protected bool attackCooldownDone;    
    [SerializeField] protected float maxAttackCooldown;
    [SerializeField] protected int attackDamage;

    // room number is found automatically based on hierarchy, but
    // it can be manually set if we decide that's better
    /*[SerializeField] */
    protected int roomNum;

    protected float destCooldown;
    protected float maxDestCooldown;
    protected bool switchingDest;

    protected GameObject player;
    [SerializeField] protected float sightRange;
    protected bool playerNear;
    protected bool playerSighted;
    protected bool gotShot = false;

    [SerializeField] protected AudioSource deathSfx;

    // all enemies have the same start function
    void Start()
    {
        SetRoomNum();                
        gameObject.name += "R" + roomNum;

        agent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<Player>().gameObject;
        playerNear = false;
        playerSighted = false;             

        destList.Add(destination1.position);
        destList.Add(destination2.position);
        agent.destination = destination1.position;
        // in seconds
        destCooldown = 0f;
        maxDestCooldown = 0.2f;
        switchingDest = false;
        speed = GetComponent<NavMeshAgent>().speed;
            
        shootSfx = GetComponent<AudioSource>();
        StartCoroutine(AttackCooldown());

        int numEnemies = FindObjectsOfType<Enemy>().Length;
        enemiesInitialized++;
        if (enemiesInitialized >= numEnemies)                    
            Door.ResetDoorCounter();

        PrintAnyNulls();
    }    

    private void PrintAnyNulls()
    {
        // check if any important serialized variables are unset
        if(health == 0)
            Debug.LogWarning("health was not set for " + gameObject.name);
        if (sightRange == 0)
            Debug.LogWarning("sightRange was not set for " + gameObject.name);
        if (maxAttackCooldown == 0)
            Debug.LogWarning("maxAttackCooldown was not set for " + gameObject.name);
        if (maxJumpDist == 0)
            Debug.LogWarning("maxJumpDist was not set for " + gameObject.name);
        if (attackDamage == 0)
            Debug.LogWarning("attackDamage was not set for " + gameObject.name);
        if (destination1 == null)
            Debug.LogWarning("destination1 was not set for " + gameObject.name);
        if (destination2 == null)
            Debug.LogWarning("destination2 was not set " + gameObject.name);
    }       

    protected void SetRoomNum()
    {
        roomNum = 1;
        // GetRootGameObjects returns the objects in scene in the order of the hierarchy
        // use the order of the hierarchy to determine which room an enemy is in
        // checkpoints and enemies in the hierarchy should be ordered based on when the player encounters them
        foreach (GameObject obj in gameObject.scene.GetRootGameObjects())
        {
            if (obj.GetComponent<Checkpoint>() != null)
                roomNum++;
            else if (obj == this.gameObject)
                break;
        }        
    }

    public int GetRoomNum()
    {
        return roomNum;
    }

    public void SetGotShot(bool wasShot)
    {
        gotShot = wasShot;
    }

    protected IEnumerator<WaitForSecondsRealtime> AttackCooldown()
    {
        yield return new WaitForSecondsRealtime(maxAttackCooldown);
        attackCooldownDone = true;
    }

    protected float DistToPlayer()
    {
        return Vector3.Distance(this.transform.position, player.transform.position);
    }

    protected bool PlayerIsNearby()
    {
        if (DistToPlayer() < sightRange)
            return true;
        return false;
    }

    public bool PlayerIsSighted()
    {
        RaycastHit hit;
        Vector3 direction = (Player.player.transform.position - transform.position).normalized;
        // draw a raycast from enemy to player to see if player is sighted
        if (Physics.Raycast(transform.position, direction, out hit, Mathf.Infinity))
        {
            //Debug.Log("hit.name: " + hit.transform.gameObject.name);            
            if (hit.transform.gameObject.name == "Player")
                return true;
        }
        return false;
    }

    protected void FindNewDest()
    {
        List<Vector3> possibleDests = new List<Vector3>();

        foreach (Vector3 destPos in destList)
        {
            if (destPos == null)
                continue;

            bool isDestArrived = (destPos.x == agent.destination.x && destPos.z == agent.destination.z);
            if (!isDestArrived)
                possibleDests.Add(destPos);
        }

        int closestDestIndex = 0;
        float minDist = Mathf.Infinity;

        for (int i = 0; i < possibleDests.Count; i++)
        {
            float dist = Vector3.Distance(transform.position, possibleDests[i]);
            if (dist < minDist)
            {
                minDist = dist;
                closestDestIndex = i;
            }
        }
        agent.destination = possibleDests[closestDestIndex];
    }

    // called from inherited TakeDamage function
    protected override void Death()
    {
        if(deathSfx != null)
            deathSfx.Play();

        enemiesInRoom--;
        Door.SetDoorCounter(enemiesInRoom);
        if (enemiesInRoom <= 0)
            Door.RaiseDoors();
        //waits 1 second and then calls DeathCleanup()
        Invoke("DeathCleanup", 1);
    }

    protected void DeathCleanup()
    {
        Destroy(this.gameObject);
    }

    public int GetHealth()
    {
        return health;
    }

    public override void TakeDamage(int damage)
    {
        health -= damage;
        if (health == 0)
            Death();
    }
}
