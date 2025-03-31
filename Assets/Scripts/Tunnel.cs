using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tunnel : MonoBehaviour
{
    private static AudioSource bellWarningSfx;
    private float railroadSpeed;
    private float maxX;
    private static Animator flashAnimator;    

    void Start()
    {
        railroadSpeed = RailroadManager.railroadSpeed;
        maxX = RailroadManager.maxX;
        bellWarningSfx = GetComponent<AudioSource>();
        if (bellWarningSfx != null && !bellWarningSfx.isPlaying)
            bellWarningSfx.Play();

        if (GameObject.Find("HUD") == null)
        {
            Debug.LogWarning("did not find HUD");
        }
        else if (GameObject.Find("HUD").transform.Find("TunnelFlash") == null)
        {
            Debug.LogWarning("did not find tunnel flash obj");
        }
        else if (GameObject.Find("HUD").transform.Find("TunnelFlash").GetComponent<Animator>() == null)
        {
            Debug.LogWarning("did not find tunnel animator");
        }
        else
        {
            flashAnimator = GameObject.Find("HUD").transform.Find("TunnelFlash").GetComponent<Animator>();            
            if (GameManager.tunnelFlashing){
                flashAnimator.Play("TunnelFlash");
            }
        }        
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

    void Update()
    {
        Vector3 newPos = transform.position;
        newPos.x += railroadSpeed * Time.deltaTime;
        transform.position = newPos;

        if (transform.position.x > maxX)        
            Destroy(this.gameObject);        
    }
}
