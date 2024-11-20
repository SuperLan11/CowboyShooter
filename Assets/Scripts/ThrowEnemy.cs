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
        Debug.Log("newTnt vel is: " + newTnt.GetComponent<Rigidbody>().velocity);
        //newTnt.GetComponent<Rigidbody>().velocity = new Vector3(0, yThrowVel, 0);

        //Vector3 playerDirection = (player.transform.position - transform.position).normalized;
        //newTnt.GetComponent<Rigidbody>().velocity += playerDirection * maxThrowSpeed * distMult;        
    }

    private Vector3 VelToClearWall()
    {
        RaycastHit hit;
        //Vector3 direction = transform.forward;
        Vector3 playerDirection = (player.transform.position - transform.position).normalized;   
        
        Vector3 levelPlayerPos = player.transform.position;
        levelPlayerPos.y = transform.position.y;
        float distToLevelPlayer = Vector3.Distance(transform.position, levelPlayerPos);

        float distToPlayer = Vector3.Distance(transform.position, player.transform.position);                
        bool isValidThrow = false;
        bool hitWall = (Physics.Raycast(transform.position, playerDirection, out hit, sightRange));

        // y0= 9.8*sqrt(wallHeight/4.9). initial y velocity needed to max y position at wallHeight
        // landTime= 2*sqrt(wallHeight/4.9). time from throwing to hit player, assuming y diff is 0
        // x0= horDist/landTime. initial x velocity needed to reach player by time
        // check if y(dist/totalDist * landTime) >= wallTop

        float halfTntHeight = tntPrefab.GetComponent<CapsuleCollider>().height / 2;
        Debug.Log("half tnt height: " + halfTntHeight);
        float relativeWallHeight = hit.collider.bounds.max.y + halfTntHeight*2 - transform.position.y;
        // increase heightDiff until valid throw or angle limit exceeded
        float yV0 = 9.8f * Mathf.Sqrt(relativeWallHeight / 4.9f);
        float landTime = 2f * Mathf.Sqrt(relativeWallHeight / 4.9f);
        //float xV0 = distToLevelPlayer / landTime;
        float xV0 = (distToLevelPlayer / landTime) * playerDirection.x;
        float zV0 = (distToLevelPlayer / landTime) * playerDirection.z;
        
        float distToWall = Vector3.Distance(transform.position, hit.point);
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

            //relativeWallHeight = hit.collider.bounds.max.y + halfTntHeight - transform.position.y;
            adjustedWallHeight += wallHeightInc;

            // use sin and cos to get new yV0 and xV0? no, wallHeight
            yV0 = 9.8f * Mathf.Sqrt(adjustedWallHeight / 4.9f);
            landTime = 2f * Mathf.Sqrt(adjustedWallHeight / 4.9f);
            //float xV0 = distToLevelPlayer / landTime;
            //xV0 = (distToLevelPlayer / landTime) * playerDirection.x;
            xV0 = (distToPlayer / landTime) * playerDirection.x;
            //zV0 = (distToLevelPlayer / landTime) * playerDirection.z;
            zV0 = (distToPlayer / landTime) * playerDirection.z;

            Debug.Log("yV0: " + yV0);

            distToWall = Vector3.Distance(transform.position, hit.point);
            t = (distToWall / distToPlayer) * landTime;
            // how to get distToEndOfWall?

            aboveWallStart = (yV0 * t - 4.9f * t * t > relativeWallHeight);
            if (aboveWallStart)
            {
                Debug.Log("angle " + angle + " was valid throw");
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
            Debug.Log("going to player");
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
