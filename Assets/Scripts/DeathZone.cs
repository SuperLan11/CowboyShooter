using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private MeshRenderer myMesh;

    // Start is called before the first frame update
    void Start()
    {
        myMesh = GetComponent<MeshRenderer>();
        myMesh.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {        
        if(other.gameObject.name == "Player")
        {
            // kill player by getting their hp so Death can stay protected
            int playerHp = Player.player.GetHealth();
            Player.player.TakeDamage(playerHp);
        }
        // in case enemy somehow falls off edge (idk how)
        else if(other.gameObject.name.Contains("Enemy"))
        {
            int enemyHp = other.GetComponent<Enemy>().GetHealth();
            other.GetComponent<Enemy>().TakeDamage(enemyHp);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
