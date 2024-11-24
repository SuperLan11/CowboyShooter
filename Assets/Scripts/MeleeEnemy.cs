using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemy : Enemy
{    
    [SerializeField] private float recoilTime = 0.75f;
    [SerializeField] private float recoilDist = 2f;
    [SerializeField] private float recoilAccel = 0.05f;
    private bool inRecoil = false;
    private Vector3 recoilPos;

    [SerializeField] private AudioSource daggerSfx;    
    
    private void OnCollisionEnter(Collision collision)
    {                
        for(int i = 0; i < collision.contactCount; i++)
        {
            bool hitPlayer = collision.GetContact(i).otherCollider.gameObject.name == "Player";            
            if (hitPlayer && attackCooldownDone)
            {                
                Strike(Player.player);
                inRecoil = true;
                recoilPos = transform.position - transform.forward * recoilDist;
                attackCooldownDone = false;               
                StartCoroutine(AttackCooldown());
                StartCoroutine(StopForTime(recoilTime));
                break;
            }
        }        
    }

    private IEnumerator StopForTime(float seconds)
    {                
        GetComponent<NavMeshAgent>().velocity = Vector3.zero;          
        yield return new WaitForSecondsRealtime(seconds);        
        inRecoil = false;        
    }

    private void Strike(Player player)
    {
        if (isDead)        
            return;        

        if (daggerSfx != null)
            daggerSfx.Play();

        player.TakeDamage(attackDamage);                
    } 

    void Update()
    {        
        if (!switchingDest && agent.remainingDistance <= 0.01f)
        {            
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
            agent.destination = player.transform.position;
        }
        else if (agent.destination == player.transform.position)
        {            
            gotShot = false;
            FindNewDest();
        }

        if (inRecoil)
            transform.position = Vector3.Lerp(transform.position, recoilPos, recoilAccel);
    }    
}
