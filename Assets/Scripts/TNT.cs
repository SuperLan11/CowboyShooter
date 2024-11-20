using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNT : MonoBehaviour
{
    [SerializeField] private float TtlOnHit;
    [SerializeField] private AudioSource sizzleSfx;
    [SerializeField] private int explodeDamage = 1;
    [SerializeField] private float explodeRadius = 5f;
    [SerializeField] private GameObject explosionPrefab;

    private AudioSource boomSfx;    

    // Start is called before the first frame update
    void Start()
    {   
        // boom sfx is on enemy since TNT object will be destroyed
        boomSfx = FindAnyObjectByType<ThrowEnemy>().boomSfx;
        if(sizzleSfx != null)
            sizzleSfx.Play();
    }

    private void OnCollisionEnter(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).otherCollider.tag == "FLOOR")            
                StartCoroutine(TTL(TtlOnHit));            
        }        
    }

    private IEnumerator TTL(float seconds)
    {
        yield return new WaitForSeconds(seconds);        

        if (sizzleSfx != null && sizzleSfx.isPlaying)
            sizzleSfx.Stop();
        
        if (boomSfx != null)
            boomSfx.Play();

        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        if (Vector3.Distance(transform.position, Player.player.transform.position) < explodeRadius)                    
            Player.player.TakeDamage(explodeDamage);        
        
        Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
