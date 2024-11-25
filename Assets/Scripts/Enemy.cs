/*
@Authors - Patrick, Landon
@Description - Enemy class. Different enemy prefabs should be able to use the same script
!There should be be NO PRIVATE METHODS in this class! Only protected and public.
*/

//pointless comment

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
    [SerializeField] public static float maxJumpDist = 2f;

    public static int enemiesInitialized = 0;
    public static int enemiesInRoom = 0;

    // attackCooldown var not needed as coroutines are used
    protected bool attackCooldownDone;
    [SerializeField] protected float maxAttackCooldown;
    [SerializeField] protected int attackDamage;

    public int checkpointNum;

    protected float destCooldown;
    protected float maxDestCooldown;
    protected bool switchingDest;

    protected GameObject player;
    [SerializeField] protected float sightRange;
    protected bool playerNear;
    protected bool playerSighted;
    [System.NonSerialized] public bool gotShot = false;
    protected bool isDead = false;

    [SerializeField] protected AudioSource deathSfx;    

    // all enemies have the same start function
    void Start()
    {               
        enemiesInitialized++;        
        int numEnemies = FindObjectsOfType<Enemy>().Length;
        if (enemiesInitialized >= numEnemies)
            Door.ResetDoorCounter();

        // destroy enemies in completed rooms, but not enemies in future rooms
        // doing this at the start of Start() makes scene reloading faster
        if (checkpointNum < GameManager.currentCheckpoint)
        {            
            Destroy(this.gameObject);
            // the script still runs even after the gameObject is destroyed            
            return;
        }

        agent = GetComponent<NavMeshAgent>();
        player = Player.player.gameObject;
        playerNear = false;
        playerSighted = false;

        destList.Add(destination1.position);
        destList.Add(destination2.position);
        agent.destination = destination1.position;
        
        // in seconds
        destCooldown = 0f;
        maxDestCooldown = 0.4f;
        switchingDest = false;        
            
        shootSfx = GetComponent<AudioSource>();
        StartCoroutine(AttackCooldown());
       
        //PrintAnyNulls();
    }

    private void PrintAnyNulls()
    {
        // check if any important serialized variables are unset
        if (health == 0)
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
        
        Vector3 newDest = possibleDests[closestDestIndex];

        RaycastHit hit;
        Vector3 beneathDest = new Vector3(newDest.x, newDest.y - 1, newDest.z);
        Vector3 downDirection = (beneathDest - newDest).normalized;
        if (Physics.Raycast(newDest, downDirection, out hit, Mathf.Infinity))
            agent.destination = hit.point;
        else
            agent.destination = newDest;
    }

    // called from inherited TakeDamage function
    protected override void Death()
    {
        isDead = true;
        //disables all components and re-enables death SFX
        DisableAllComponents();
        deathSfx.enabled = true;
        
        if(deathSfx != null)
            deathSfx.Play();

        enemiesInRoom--;
                
        Door.SetDoorCounter(enemiesInRoom);
        if (enemiesInRoom <= 0)
        {           
            Door.RaiseDoors();
        }
        
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
        if (health <= 0)
            Death();
    }

    protected void DisableAllComponents()
    {
        MonoBehaviour[] components = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour c in components)
        {
            if (c != null && c != deathSfx)
            {
                c.enabled = false;
            }
        }

        NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        MeshRenderer meshRenderer= GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }

        CapsuleCollider capsuleColldier= GetComponent<CapsuleCollider>();
        if (capsuleColldier != null)
        {
            capsuleColldier.enabled = false;
        }

        GameObject[] childObjects = GetAllChildren(gameObject);
        foreach (GameObject child in childObjects)
        {
            MeshRenderer childMeshRenderer = child.GetComponent<MeshRenderer>();
            if (childMeshRenderer != null)
            {
                childMeshRenderer.enabled = false;
            }
        }
    }

    protected GameObject[] GetAllChildren(GameObject parent)
    {
        Transform[] childTransforms = parent.GetComponentsInChildren<Transform>();
        GameObject[] children = new GameObject[childTransforms.Length - 1];
        int index = 0;

        foreach (Transform child in childTransforms)
        {
            if (child.gameObject != parent)
            {
                children[index] = child.gameObject;
                index++;
            }
        }
        return children;
    }
}
