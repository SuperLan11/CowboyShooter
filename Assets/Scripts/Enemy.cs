/*
@Authors - Patrick, Landon
@Description - Enemy class. Different enemy prefabs should be able to use the same script
*/

using System.Collections;
using System.Collections.Generic;
using Unity.AI;
using UnityEngine;

using UnityEngine.AI;
using UnityEngine.Scripting.APIUpdating;
using Vector3 = UnityEngine.Vector3;
using TMPro;

public class Enemy : Character
{
    private NavMeshAgent agent;

    // make these Transform since Vector3 can't be dragged in inspector
    [SerializeField] private Transform destination1;
    [SerializeField] private Transform destination2;
    private List<Vector3> destList = new List<Vector3>();
    public static float maxJumpDist = 7f;

    public static int enemiesInitialized = 0;
    private static int enemiesAlive = 0;

    [SerializeField] public float destCooldown;
    [SerializeField] private float maxDestCooldown;
    [SerializeField] private bool switchingDest;

    private GameObject player;
    [SerializeField] private float sightRange;
    private bool playerNear;
    private bool playerSighted;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<Player>().gameObject;
        playerNear = false;
        playerSighted = false;
        sightRange = 5f;        

        destList.Add(destination1.position);
        destList.Add(destination2.position);
        agent.destination = destination1.position;
        // in seconds
        destCooldown = 0f;
        maxDestCooldown = 0.5f;
        switchingDest = false;

        shootCooldown = 0f;
        maxShootCooldown = 1f;
        // this is only here to give feedback for shooting
        shootSfx = GetComponent<AudioSource>();

        // change to find enemies in a certain room?
        int numEnemies = FindObjectsOfType<Enemy>().Length;

        enemiesInitialized++;
        enemiesAlive++;
        if (enemiesInitialized >= numEnemies)
        {
            UpdateDoorCounter();
        }
    }

    private void UpdateDoorCounter()
    {
        GameObject[] doors = GameObject.FindGameObjectsWithTag("DOOR");
        foreach (GameObject door in doors)
        {
            door.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = enemiesAlive.ToString();
        }
    }

    protected override void Shoot(GameObject player)
    {
        player.GetComponent<Player>().TakeDamage(1);
        shootSfx.Play();
    }

    public bool PlayerSighted(Vector3 enemyPos)
    {
        RaycastHit hit;
        Vector3 direction = (Camera.main.transform.position - transform.position).normalized;
        // draw a raycast from enemy to player to see if player is sighted
        if (Physics.Raycast(transform.position, direction, out hit, Mathf.Infinity))
        {
            //Debug.Log("hit.name: " + hit.transform.gameObject.name);            
            if (hit.transform.gameObject.name == "Player")
                return true;
        }
        return false;
    }

    private void FindNewDest(Vector3 destArrived)
    {
        List<Vector3> possibleDests = new List<Vector3>();

        foreach (Vector3 destPos in destList)
        {
            bool isDestArrived = (destPos.x == destArrived.x && destPos.z == destArrived.z);
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
        Destroy(this.gameObject);
        enemiesAlive--;
        UpdateDoorCounter();
        if (enemiesAlive < 0)
            Door.RaiseDoors();
    }

    void Update()
    {
        if (!switchingDest && agent.remainingDistance <= 0.01f)
        {
            //Debug.Log("got to dest, find new dest");
            switchingDest = true;
            FindNewDest(agent.destination);
            return;
        }
        else if (switchingDest)
        {
            destCooldown += Time.deltaTime;
            if (destCooldown >= maxDestCooldown)
            {
                switchingDest = false;
                destCooldown = 0f;
            }
            return;
        }

        if (Vector3.Distance(this.transform.position, player.transform.position) < sightRange)
            playerNear = true;
        else
            playerNear = false;

        playerSighted = PlayerSighted(transform.position);

        if (playerNear && playerSighted)
        {
            //Debug.Log("going to player");
            agent.destination = player.transform.position;

            if (shootCooldown >= maxShootCooldown)
            {
                Shoot(player);
                shootCooldown = 0f;
            }
        }
        else if (agent.destination == player.transform.position)
        {
            //Debug.Log("Find dest other than player");
            FindNewDest(agent.destination);
        }

        shootCooldown += Time.deltaTime;
    }
}
