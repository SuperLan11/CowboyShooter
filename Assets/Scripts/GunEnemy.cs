using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunEnemy : Enemy
{         
    private Vector3 shootPos;

    protected void Shoot(GameObject player)
    {       
        if (isDead)
        {
            return;
        }
        
        player.GetComponent<Player>().TakeDamage(attackDamage);        
        if (shootSfx != null)
            shootSfx.Play();        
    }

    private Vector3 ShootPos()
    {
        Vector3 playerDirection = (player.transform.position - transform.position).normalized;        
        float distToPlayer = DistToPlayer();
        
        RaycastHit hit;
        // raycast backward to see if navmesh would move within 0.5 of wall
        // prevents enemies from moving into walls
        bool hitObjBackward = Physics.Raycast(transform.position, -1*playerDirection, out hit, minDistFromWall);
        if (hitObjBackward && hit.transform.tag == "WALL")        
            return transform.position;        
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
                attackCooldownDone = false;
                StartCoroutine(AttackCooldown());
            }            
        }
        else if (agent.destination == shootPos)
        {
            //Debug.Log("Find dest other than player");
            gotShot = false;
            FindNewDest();
        }
    }
}
