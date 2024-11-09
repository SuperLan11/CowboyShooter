/*
@Authors - Patrick, Landon
@Description - Enemy class. Different enemy prefabs should be able to use the same script
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;
using UnityEngine.Scripting.APIUpdating;
using Vector3 = UnityEngine.Vector3;

public class Enemy : Character
{    
    private NavMeshAgent agent;
    
    [SerializeField] private Transform destination1;
    [SerializeField] private Transform destination2;    
    private int curDestination;
    private const int PLAYER_DEST = 3;

    [SerializeField] private GameObject player;
    [SerializeField] private float sightRange;
    private bool playerNear;
    private bool playerSighted;

    void Start()
    {        
        agent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<Player>().gameObject;
        playerNear = false;
        playerSighted = false;

        agent.destination = destination1.position;
        curDestination = 1;

        shootCooldown = 0f;
        maxShootCooldown = 1f;
        // this is only here to give feedback for shooting
        shootSfx = GetComponent<AudioSource>();
    }
    
    protected override void Shoot(GameObject player)
    {
        Debug.Log("SHOOTING");
        player.GetComponent<Player>().TakeDamage(1);
        shootSfx.Play();        
    }    

    //provided we have a trigger collider for detecting player
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PLAYER")
        {
            agent.destination = player.transform.position;
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "PLAYER")
        {
            agent.destination = destination1.transform.position;
        }
    }

    public bool PlayerSighted(Vector3 enemyPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(enemyPos);
        RaycastHit hit;

        //if (Physics.Raycast(ray, out hit))
        Vector3 direction = (Camera.main.transform.position - transform.position).normalized;
        if(Physics.Raycast(transform.position, direction, out hit, Mathf.Infinity))
        {
            //Debug.Log("hit.name: " + hit.transform.gameObject.name);            
            if (hit.transform.gameObject.name == "Player")                            
                return true;            
        }                    
        return false;
    }

    private void SetClosestDest()
    {
        Transform[] destinations = { destination1, destination2 };
        int closestDestIndex = 0;
        float minDist = Mathf.Infinity;

        for(int i = 0; i < destinations.Length; i++)
        {
            float dist = Vector3.Distance(transform.position, destinations[i].position);
            if (dist < minDist)
            {
                minDist = dist;
                closestDestIndex = i;
            }
        }
        curDestination = closestDestIndex + 1;
        agent.destination = destinations[closestDestIndex].position;
    }


    void Update()
    {
        if (Vector3.Distance(this.transform.position, player.transform.position) < sightRange)
            playerNear = true;
        else
            playerNear = false;

        // how to check if enemy can raycast player?
        playerSighted = PlayerSighted(transform.position);

        if(playerNear && playerSighted)
        {            
            agent.destination = player.transform.position;
            curDestination = PLAYER_DEST;            

            if (shootCooldown >= maxShootCooldown)
            {
                Shoot(player);
                shootCooldown = 0f;
            }
        }
        else if(curDestination == PLAYER_DEST)
        {
            SetClosestDest();
        }

        shootCooldown += Time.deltaTime;
        
        if (curDestination == 1 && agent.remainingDistance <= 0.01)
        {
            curDestination = 2;
            agent.destination = destination2.position;
        }
        else if(curDestination == 2 && agent.remainingDistance <= 0.01)
        {
            curDestination = 1;
            agent.destination = destination1.position;
        }
    }
}
