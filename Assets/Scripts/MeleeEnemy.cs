using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemy : Enemy
{
    [SerializeField] private AudioSource daggerSfx;
    [SerializeField] private float recoilTime = 0.5f;

    private void OnCollisionEnter(Collision collision)
    {        
        for(int i = 0; i < collision.contactCount; i++)
        {
            bool hitPlayer = collision.GetContact(i).otherCollider.gameObject.name == "Player";
            // change to attack cooldown later
            if (hitPlayer && attackCooldown >= maxAttackCooldown)
            {
                Strike(Player.player);                
                attackCooldown = 0f;
                StartCoroutine(StopForTime(recoilTime));
            }
        }
    }

    private IEnumerator StopForTime(float seconds)
    {
        float prevSpeed = GetComponent<NavMeshAgent>().speed;
        GetComponent<NavMeshAgent>().speed = 0f;
        //RecoilBack();
        yield return new WaitForSecondsRealtime(seconds);
        GetComponent<NavMeshAgent>().speed = prevSpeed;
    }

    private void RecoilBack()
    {
        transform.position = Vector3.Lerp(transform.position, transform.position + transform.forward * -30, 0.002f);
    }

    private void Strike(Player player)
    {
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
        playerSighted = PlayerIsSighted();

        if (playerNear && playerSighted)
        {
            Debug.Log("going to player");
            agent.destination = player.transform.position;
        }
        else if (agent.destination == player.transform.position)
        {
            //Debug.Log("Find dest other than player");
            FindNewDest(agent.destination);
        }

        attackCooldown += Time.deltaTime;
    }
}
