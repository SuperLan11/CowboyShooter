using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemy : Enemy
{    
    [SerializeField] private float recoilTime = 0.75f;
    [SerializeField] private AudioSource daggerSfx;    

    private void OnCollisionEnter(Collision collision)
    {                
        for(int i = 0; i < collision.contactCount; i++)
        {
            bool hitPlayer = collision.GetContact(i).otherCollider.gameObject.name == "Player";            
            if (hitPlayer && attackCooldownDone)
            {                
                Strike(Player.player);
                attackCooldownDone = false;
                StartCoroutine(AttackCooldown());
                StartCoroutine(StopForTime(recoilTime));
            }
        }
    }

    private IEnumerator StopForTime(float seconds)
    {        
        // how to immediately stop NavMesh? this has some ease out
        //GetComponent<NavMeshAgent>().speed = 0f;
        GetComponent<NavMeshAgent>().velocity = Vector3.zero;
        GetComponent<NavMeshAgent>().isStopped = true;
        yield return new WaitForSecondsRealtime(seconds);
        GetComponent<NavMeshAgent>().isStopped = false;
        //GetComponent<NavMeshAgent>().speed = speed;
    }

    private void RecoilBack()
    {
        transform.position = Vector3.Lerp(transform.position, transform.position + transform.forward * -30, 0.002f);
    }

    private void Strike(Player player)
    {
        if (isDead){
            return;
        }
        
        if (daggerSfx != null)
            daggerSfx.Play();

        player.TakeDamage(attackDamage);                
    }

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
            agent.destination = player.transform.position;
        }
        else if (agent.destination == player.transform.position)
        {
            //Debug.Log("Find dest other than player");
            gotShot = false;
            FindNewDest();
        }
    }
}
