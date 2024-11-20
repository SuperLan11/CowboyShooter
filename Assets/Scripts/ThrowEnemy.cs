using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowEnemy : Enemy
{
    [SerializeField] private GameObject tntPrefab;    
    [SerializeField] private Transform tntSpawn;    

    [SerializeField] private float yThrowVel = 6f;
    [SerializeField] private float maxThrowSpeed = 7f;

    [SerializeField] private GameObject tntChild;

    [SerializeField] public AudioSource boomSfx;

    [SerializeField] private float wallHeightInc;

    private void ThrowTNT()
    {
        // tntChild is not a TNT prefab. 
        // This prevents weird link issues and makes the tnt the enemy holds stay in place
        // The tnt held is hidden and a new tnt object is created to appear to be throwing a tnt        
        tntChild.GetComponent<MeshRenderer>().enabled = false;

        float distToPlayer = DistToPlayer();
        float distMult = distToPlayer / sightRange;
        
        GameObject newTnt = Instantiate(tntPrefab, tntSpawn.position, Quaternion.identity);
        //float yVel = VelToClearWall();
        Vector3 tntVel = VelToClearWall();
        newTnt.GetComponent<Rigidbody>().velocity = tntVel;
        //Debug.Log("newTnt vel is: " + newTnt.GetComponent<Rigidbody>().velocity);                
    }

    private Vector3 VelToClearWall()
    {
        RaycastHit hit;
        RaycastHit straightHit;
        //Vector3 direction = transform.forward;
        Vector3 playerDirection = (player.transform.position - tntSpawn.position).normalized;   
        
        Vector3 levelPlayerPos = player.transform.position;
        levelPlayerPos.y = tntSpawn.position.y;
        float distToLevelPlayer = Vector3.Distance(tntSpawn.position, levelPlayerPos);

        float distToPlayer = Vector3.Distance(tntSpawn.position, player.transform.position);                
        bool isValidThrow = false;
        // consider non-wall objects laters
        bool hitWall = (Physics.Raycast(tntSpawn.position, playerDirection, out hit, sightRange));

        bool hitStraightToWall = (Physics.Raycast(tntSpawn.position, tntSpawn.forward, out straightHit, sightRange));
        float distStraightToWall = Vector3.Distance(tntSpawn.position, straightHit.point);

        // y0= 9.8*sqrt(wallHeight/4.9). initial y velocity needed to max y position at wallHeight
        // landTime= 2*sqrt(wallHeight/4.9). time from throwing to hit player, assuming y diff is 0
        // x0= horDist/landTime. initial x velocity needed to reach player by time
        // check if y(dist/totalDist * landTime) >= wallTop

        float halfTntHeight = tntPrefab.transform.localScale.z * tntPrefab.GetComponent<CapsuleCollider>().height / 2;        
        // account for width of tnt
        float relativeWallHeight = hit.collider.bounds.max.y + halfTntHeight + 0.05f - tntSpawn.position.y;
        // increase heightDiff until valid throw or angle limit exceeded
        float yV0 = 9.8f * Mathf.Sqrt(relativeWallHeight / 4.9f);
        float landTime = 2f * Mathf.Sqrt(relativeWallHeight / 4.9f);
        //float xV0 = distToLevelPlayer / landTime;
        float xV0 = (distToPlayer / landTime) * playerDirection.x;
        float zV0 = (distToPlayer / landTime) * playerDirection.z;
        
        float distToWall = Vector3.Distance(tntSpawn.position, hit.point);
        float t = (distToWall / distToPlayer) * landTime;
        // how to get distToEndOfWall?

        bool aboveWallStart = (yV0 * t - 4.9f * t * t > relativeWallHeight);
        //bool aboveWallEnd = (yV0 * )
        // get angle to throw to just pass wall
        float angle = Mathf.Atan(yV0 / xV0);
        float adjustedWallHeight = relativeWallHeight;        

        while(!isValidThrow && angle <= 89)
        {
            angle += (100f * Mathf.Deg2Rad);
            adjustedWallHeight = distStraightToWall * Mathf.Tan(angle);

            yV0 = 9.8f * Mathf.Sqrt(adjustedWallHeight / -Physics.gravity.y) + 0.15f;
            landTime = 2f * Mathf.Sqrt(adjustedWallHeight / -Physics.gravity.y);                        
            xV0 = (distToPlayer / landTime) * playerDirection.x - 0.03f;            
            zV0 = (distToPlayer / landTime) * playerDirection.z - 0.03f;

            distToWall = Vector3.Distance(tntSpawn.position, hit.point);
            t = (distToWall / distToPlayer) * landTime;
            // how to get distToEndOfWall?

            aboveWallStart = (yV0 * t - (4.9f * t * t) > relativeWallHeight);
            if (aboveWallStart)
            {
                //Debug.Log("angle " + angle*Mathf.Deg2Rad + " was valid throw");
                isValidThrow = true;                
            }
        }

        return new Vector3(xV0, yV0, zV0);                
    }

    // Update is called once per frame
    void Update()
    {
        //VelToClearWall();

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

        playerNear = PlayerIsNearby();
        playerSighted = PlayerIsSighted(transform.position);

        if (playerNear)
        {
            //Debug.Log("going to player");
            agent.destination = player.transform.position;

            if (attackCooldown >= maxAttackCooldown)
            {
                ThrowTNT();                
                attackCooldown = 0f;
            }
        }
        else if (agent.destination == player.transform.position)
        {
            //Debug.Log("Find dest other than player");
            FindNewDest(agent.destination);
        }

        if (attackCooldown >= maxAttackCooldown / 2)
            tntChild.GetComponent<MeshRenderer>().enabled = true;

        attackCooldown += Time.deltaTime;
    }
}
