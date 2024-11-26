using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tunnel : MonoBehaviour
{
    private static AudioSource bellWarningSfx;

    // Start is called before the first frame update
    void Start()
    {
        bellWarningSfx = GetComponent<AudioSource>();
        if(bellWarningSfx != null && !bellWarningSfx.isPlaying)
            bellWarningSfx.Play();
    }

    private void OnCollisionEnter(Collision collision)
    {
        for(int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).otherCollider.transform.name == "Player")
            {                
                Player.player.TakeDamage(Player.player.GetHealth());
                break;
            }
        }        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
