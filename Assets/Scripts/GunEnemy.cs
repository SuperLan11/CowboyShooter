using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunEnemy : Enemy
{         
    // shootPos is where enemy will (usually) stand to shoot player
    private Vector3 shootPos;    
    [SerializeField] private Transform bulletSpawn;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;

    protected void Shoot(GameObject player)
    {       
        if (isDead)
        {
            return;
        }

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
        Vector3 playerDirection = (player.transform.position - bulletSpawn.position).normalized;
        bullet.GetComponent<Rigidbody>().velocity = playerDirection * bulletSpeed;

        if (shootSfx != null)
            shootSfx.Play();
    }

    private Vector3 ShootPos()
    {
        Vector3 playerDirection = (player.transform.position - transform.position).normalized;        
        float distToPlayer = DistToPlayer();
        // NavMesh accounts for if the enemy can't go to shoot pos while against a wall
        return transform.position + playerDirection * -(sightRange - 2 - distToPlayer);
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
            gotShot = false;
            FindNewDest();
        }
    }
}
