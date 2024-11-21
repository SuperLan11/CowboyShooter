using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNT : MonoBehaviour
{
    [SerializeField] private float TtlOnHit;
    [SerializeField] private AudioSource sizzleSfx;
    [SerializeField] private int explodeDamage = 1;    
    [SerializeField] private GameObject explosionPrefab;
    private float explodeRadius;

    private AudioSource boomSfx;    

    // Start is called before the first frame update
    void Start()
    {
        explodeRadius = explosionPrefab.GetComponent<Explosion>().explodeRadius;

        // boom sfx is on enemy since TNT object will be destroyed
        boomSfx = FindAnyObjectByType<ThrowEnemy>().boomSfx;
        if(sizzleSfx != null)
            sizzleSfx.Play();
    }

    private void OnCollisionEnter(Collision collision)
    {
        bool hitFloor, hitPlayer;
        for (int i = 0; i < collision.contactCount; i++)
        {
            hitFloor = collision.GetContact(i).otherCollider.tag == "FLOOR";
            hitPlayer = collision.GetContact(i).otherCollider.gameObject.name == "Player";
            if (hitPlayer || hitFloor)
            {
                StartCoroutine(TTL(TtlOnHit));
                break;
            }
        }        
    }

    private bool PlayerExposed()
    {
        RaycastHit hit;        
        Vector3 playerDirection = (Player.player.transform.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, playerDirection, out hit, Mathf.Infinity))
        {
            if (hit.transform.gameObject.name == "Player")
                return true;
        }
        return false;
    }

    private IEnumerator TTL(float seconds)
    {
        yield return new WaitForSeconds(seconds);        

        if (sizzleSfx != null && sizzleSfx.isPlaying)
            sizzleSfx.Stop();
        
        if (boomSfx != null)
            boomSfx.Play();

        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        if (Vector3.Distance(transform.position, Player.player.transform.position) < explodeRadius &&
            PlayerExposed())        
            Player.player.TakeDamage(explodeDamage);                    
        
        Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
