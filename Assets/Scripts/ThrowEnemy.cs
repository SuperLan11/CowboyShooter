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

    private void ThrowTNT()
    {
        // tntChild is not a TNT prefab. 
        // This prevents weird link issues and makes the tnt the enemy holds stay in place
        // The tnt held is hidden and a new tnt object is created to appear to be throwing a tnt        
        tntChild.GetComponent<MeshRenderer>().enabled = false;

        float distToPlayer = DistToPlayer();
        float distMult = distToPlayer / sightRange;
        
        GameObject newTnt = Instantiate(tntPrefab, tntSpawn.position, Quaternion.identity);
        float yVel = VelToClearWall();
        newTnt.GetComponent<Rigidbody>().velocity = new Vector3(0, yVel, 0);
        //newTnt.GetComponent<Rigidbody>().velocity = new Vector3(0, yThrowVel, 0);

        Vector3 playerDirection = (player.transform.position - transform.position).normalized;
        newTnt.GetComponent<Rigidbody>().velocity += playerDirection * maxThrowSpeed * distMult;

        VelToClearWall();
    }

    private float VelToClearWall()
    {
        RaycastHit hit;
        //Vector3 direction = transform.forward;
        Vector3 direction = (player.transform.position - transform.position).normalized;   
        
        Vector3 levelPlayerPos = player.transform.position;
        levelPlayerPos.y = transform.position.y;
        float distToLevelPlayer = Vector3.Distance(transform.position, levelPlayerPos);

        float distToPlayer = Vector3.Distance(transform.position, player.transform.position);        
        float angle = Mathf.Acos(distToLevelPlayer/distToPlayer);
        angle = angle * Mathf.Rad2Deg;
        Debug.Log("angle to player: " + angle);
        //float angle = 0;

        while (Physics.Raycast(transform.position, direction, out hit, sightRange) && angle <= 88f)
        {            
            angle += (200f * Mathf.Deg2Rad);
            float distToHit = Vector3.Distance(transform.position, hit.point);

            // sin(angle) = yDifference/distToHit
            // yDifference = distToHit * sin(angle)
            direction = hit.point - transform.position;
            direction.y = distToHit * Mathf.Sin(angle*Mathf.Deg2Rad);
            direction = direction.normalized;

            GameObject hitObj = Instantiate(new GameObject(), hit.point, Quaternion.identity);
            hitObj.name = "LastHit" + angle;
            Debug.Log("last angle: " + angle);
        }
        //Debug.Log("last hit: " + hit.point);        
        // figure out how to convert this to velocity later
        return angle / 3.5f;
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
