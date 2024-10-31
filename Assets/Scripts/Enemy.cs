/*
@Authors - Patrick
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
    //navmesh crap
    [SerializeField] private GameObject destination;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject startingDestination;
    [SerializeField] private GameObject playerDestination;
    
    //!Implement this ASAP!
    protected void Shoot(){
        //shoot logic
    }

    private void Move(){
        //move logic
    } 

    void Start()
    {
        destination = startingDestination;
    }

    void Update()
    {
        agent.destination = destination.transform.position;
        Move();
    }

    //provided we have a trigger collider for detecting player
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PLAYER"){
            destination = playerDestination;
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "PLAYER"){
            destination = startingDestination;
        }
    }
}
