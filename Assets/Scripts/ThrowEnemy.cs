using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowEnemy : Enemy
{
    [SerializeField] private GameObject tntPrefab;    
    [SerializeField] private Transform tntSpawn;    

    [SerializeField] private float yThrowVel = 6f;
    [SerializeField] private float throwSpeed = 6f;

    [SerializeField] public AudioSource boomSfx;

    private void ThrowTNT()
    {                        
        GameObject tnt = Instantiate(tntPrefab, tntSpawn.position, Quaternion.identity);        
        // later, change tnt velocity based on distance to player
        // also, delay first shot or get normalized direction to player so you shoot correct way
        tnt.GetComponent<Rigidbody>().velocity = new Vector3(0, yThrowVel, 0);
        tnt.GetComponent<Rigidbody>().velocity += transform.forward * throwSpeed;
                
        //WaitToReplaceTNT(attackCooldown-0.05f);
    }

    private IEnumerator WaitToReplaceTNT(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        // could have rot issues because quaternion
        // place with cur transform offset
        Vector3 tntPos = transform.position;
        tntPos.x += 0.8f;
        GameObject tnt = Instantiate(tntPrefab, tntPos, Quaternion.identity);
        Debug.Log("made tnt called " + tnt.name);
        tnt.transform.SetParent(this.transform);
    }

    // Update is called once per frame
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

        playerNear = PlayerIsNearby();
        playerSighted = PlayerIsSighted(transform.position);

        if (playerNear && playerSighted)
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

        attackCooldown += Time.deltaTime;
    }
}
