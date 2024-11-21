using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunEnemy : Enemy
{         
    private Vector3 shootPos;

    protected void Shoot(GameObject player)
    {       
        player.GetComponent<Player>().TakeDamage(attackDamage);        
        if (shootSfx != null)
            shootSfx.Play();        
    }

    private Vector3 ShootPos()
    {
        RaycastHit hitFront;
        RaycastHit hitBack;
        bool hitForward = false, hitBackward = false;
        bool hitWallForward = false, hitWallBackward = false;

        Vector3 playerDirection = (player.transform.position - transform.position).normalized;
        hitForward = Physics.Raycast(transform.position, playerDirection, out hitFront, sightRange);
        if (hitForward && hitFront.transform.tag == "WALL")
            hitWallForward = true;

        hitBackward = Physics.Raycast(transform.position, -1*transform.forward, out hitBack, sightRange);
        if (hitBackward && hitBack.transform.tag == "WALL")
            hitWallBackward = true;
        
        float distToPlayer = DistToPlayer();

        // if a wall is between enemy and player, follow player until wall stops obstructing
        if (hitWallForward)
            return player.transform.position;
        else if (hitWallBackward)
            return transform.position;
        else
            return transform.position + playerDirection * -(sightRange - 1 - distToPlayer);        
    }

    // Update is called once per frame

    void Update()
    {
        if (!switchingDest && agent.remainingDistance <= 0.01f)
        {
            //Debug.Log("got to dest, find new dest");
            switchingDest = true;
            FindNewDest();
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

        playerNear = PlayerIsNearby();
        playerSighted = PlayerIsSighted();

        if (playerSighted && (playerNear || gotShot))
        {
            //Debug.Log("going to player");
            agent.destination = ShootPos();
            shootPos = agent.destination;
            transform.LookAt(player.transform);            

            if (attackCooldownDone && playerNear)
            {                
                Shoot(player);
                StartCoroutine(AttackCooldown());
                attackCooldownDone = false;         
            }            
        }
        else if (agent.destination == shootPos)
        {
            //Debug.Log("Find dest other than player");            
            FindNewDest();
        }

        if (!playerSighted)
        {
            gotShot = false;
            FindNewDest();
        }        
    }
}
