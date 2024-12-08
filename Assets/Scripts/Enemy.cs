/*
@Authors - Patrick, Landon
@Description - Enemy class. Different enemy prefabs should be able to use the same script
!There should be be NO PRIVATE METHODS in this class! Only protected and public.
*/

using System.Collections.Generic;
using UnityEngine;
using System.Collections;

using UnityEngine.AI;
using UnityEngine.InputSystem.LowLevel;
using Vector3 = UnityEngine.Vector3;

public abstract class Enemy : Character
{
    protected NavMeshAgent agent;

    // make these Transform since Vector3 can't be dragged in inspector
    [SerializeField] public Transform destination1;
    [SerializeField] public Transform destination2;
    protected List<Vector3> destList = new List<Vector3>();
    // assuming all enemies can jump the same distance
    [SerializeField] public static float maxJumpDist = 2f;

    public static int enemiesInitialized = 0;
    public static int enemiesInRoom = 0;

    // attackCooldown var not needed as coroutines are used
    protected bool attackCooldownDone;
    [SerializeField] protected float maxAttackCooldown;
    [SerializeField] protected int attackDamage;

    private List<List<Material>> originalMaterials = new List<List<Material>>();
    private List<Material> originalHatMaterials = new List<Material>();

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
    private bool inKnockback = false;
    private float beforeKnockbackYPos;

    public Vector3 spawnPos;
    [SerializeField] protected AudioSource deathSfx;

    protected Animator animator;
    private LayerMask floorLayer;

    public static List<Vector3> killedEnemySpawns = new List<Vector3>();
    public static List<string> enemiesTypesKilled = new List<string>();
    public static List<Transform> killedDest1List = new List<Transform>();
    public static List<Transform> killedDest2List = new List<Transform>();
    public static List<int> killedEnemyCps = new List<int>();

    // all enemies have the same start function
    void Start()
    {        
        spawnPos = transform.position;
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

        floorLayer = LayerMask.GetMask("Floors");
        
        rigidbody = GetComponent<Rigidbody>();     
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = Player.player.gameObject;
        playerNear = false;
        playerSighted = false;

        destList.Add(destination1.position);
        destList.Add(destination2.position);
        agent.destination = destination1.position;
        FindNewDest();

        SkinnedMeshRenderer[] skinRenders = GetComponentsInChildren<SkinnedMeshRenderer>();          
        for(int i = 0; i < skinRenders.Length; i++)
        {            
            originalMaterials.Add(new List<Material>());
            for(int j = 0; j < skinRenders[i].materials.Length; j++)
            {
                // make new material so changing the materials later doesn't get rid of the original materials
                originalMaterials[i].Add(new Material(skinRenders[i].materials[j]));
            }            
        }

        if (GetComponent<MeleeEnemy>() != null)
        {
            foreach(Material mat in GetComponentInChildren<MeshRenderer>().materials)
            {
                originalHatMaterials.Add(new Material(mat));
            }
        }

        // in seconds
        destCooldown = 0f;
        maxDestCooldown = 0.4f;
        switchingDest = false;                

        shootSfx = GetComponent<AudioSource>();
        StartCoroutine(AttackCooldown());
       
        //PrintAnyNulls();
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public void SetHealth(int hp)
    {
        health = hp;
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
        if (Physics.Raycast(newDest, downDirection, out hit, Mathf.Infinity, floorLayer))
            agent.destination = hit.point;
        else
            agent.destination = newDest;
    }

    // called from inherited TakeDamage function
    protected override void Death()
    {
        //animator.Play("Death", -1, 0f);

        isDead = true;
        //disables all components and re-enables death SFX
        //DisableAllComponents();
        //deathSfx.enabled = true;

        killedEnemySpawns.Add(this.spawnPos);
        killedDest1List.Add(this.destination1);
        killedDest2List.Add(this.destination2);
        killedEnemyCps.Add(checkpointNum);

        if (GetComponent<MeleeEnemy>() != null)
            enemiesTypesKilled.Add("Melee");
        else if (GetComponent<ThrowEnemy>() != null)
            enemiesTypesKilled.Add("Throw");
        else if (GetComponent<GunEnemy>() != null)
            enemiesTypesKilled.Add("Gun");

        if (deathSfx != null)
                deathSfx.Play();
        
        enemiesInRoom--;        
        
        Door.SetDoorCounter(enemiesInRoom);
        if (enemiesInRoom <= 0)
        {            
            Door.RaiseDoors();
        }
        
        //waits 1 second and then calls DeathCleanup()
        //Invoke("DeathCleanup", 1);
        DeathCleanup();
    }

    protected void DeathCleanup()
    {
        Destroy(this.gameObject);
    }

    public int GetHealth()
    {
        return health;
    }

    private IEnumerator FlashRed(float seconds)
    {                
        SkinnedMeshRenderer[] skins = GetComponentsInChildren<SkinnedMeshRenderer>();
        for(int i = 0; i < skins.Length; i++)
        {
            Material[] redMats = new Material[skins[i].materials.Length];
            for(int j = 0; j < skins[i].materials.Length; j++)
            {
                redMats[j] = new Material(Shader.Find("Standard"));
                redMats[j].color = Color.red;                
            }            
            skins[i].materials = redMats;
        }

        if(GetComponent<MeleeEnemy>() != null)
        {
            int numHatMats = GetComponentInChildren<MeshRenderer>().materials.Length;
            Material[] redMats = new Material[numHatMats];
            for (int j = 0; j < numHatMats; j++)
            {
                redMats[j] = new Material(Shader.Find("Standard"));
                redMats[j].color = Color.red;
            }
            GetComponentInChildren<MeshRenderer>().materials = redMats;
        }

        yield return new WaitForSeconds(seconds);
        if (!isDead)        
            ChangeColorToNormal();             
    }

    private void ChangeColorToNormal()
    {
        SkinnedMeshRenderer[] meshRenders = GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < meshRenders.Length; i++)
            meshRenders[i].materials = originalMaterials[i].ToArray();

        if (GetComponent<MeleeEnemy>() != null)        
            GetComponentInChildren<MeshRenderer>().materials = originalHatMaterials.ToArray();        
    }

    public override void TakeDamage(int damage)
    {
        health -= damage;        
        animator.Play("TakeDamage", -1, 0f);
        StartCoroutine(FlashRed(0.2f));
        if (health <= 0)
            Death();                
    }

    //if we want to go back to disabling mesh renderers, we can just uncomment those lines
    protected void DisableAllComponents()
    {
        /*
        MonoBehaviour[] components = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour c in components)
        {
            if (c != null && c != deathSfx)
            {
                c.enabled = false;
            }
        }
        */

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

        /*
        MeshRenderer meshRenderer= GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
        */

        CapsuleCollider capsuleColldier= GetComponent<CapsuleCollider>();
        if (capsuleColldier != null)
        {
            capsuleColldier.enabled = false;
        }

        /*
        GameObject[] childObjects = GetAllChildren(gameObject);
        foreach (GameObject child in childObjects)
        {
            MeshRenderer childMeshRenderer = child.GetComponent<MeshRenderer>();
            if (childMeshRenderer != null)
            {
                childMeshRenderer.enabled = false;
            }
        }
        */
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

    public void ApplyForce(Vector3 modifiedVector)
    {
        inKnockback = true;
        agent.isStopped = true;
        agent.enabled = false;
        beforeKnockbackYPos = transform.position.y;

        rigidbody.AddForce(modifiedVector, ForceMode.Impulse);
        
        transform.position = new Vector3(transform.position.x, beforeKnockbackYPos, transform.position.z);
        StartCoroutine(EnableNavMeshAgent());
    }

    //this will solve some of the errors
    private IEnumerator EnableNavMeshAgent()
    {
        yield return new WaitForSeconds(0.15f);

        agent.enabled = true;
        agent.isStopped = false;
        inKnockback = false;
    }    
}
