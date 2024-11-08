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
    }
    
    protected void Shoot()
    {
        //shoot logic
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
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Ray ray = Camera.main.ScreenPointToRay(enemyPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("raycast hit");
            return true;
        }
        return false;
    }


    void Update()
    {
        if (Vector3.Distance(this.transform.position, player.transform.position) < sightRange)
            playerNear = true;
        else
            playerNear = false;

        // how to check if enemy can raycast player?
        PlayerSighted(transform.position);

        if(playerNear && playerSighted)
        {            
            agent.destination = player.transform.position;
            curDestination = 3;
        }
        // use mesh width?        
        else if (curDestination == 1 && agent.remainingDistance <= 0.01)
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
