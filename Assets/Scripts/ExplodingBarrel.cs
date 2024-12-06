/*
@Authors - Patrick
@Description - Script that defines when a barrel explodes. Mainly relies on Landon's explosion script
datafields.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodingBarrel : MonoBehaviour
{
    private int explodeDamage = 3;    
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private AudioSource explosionSfx;
    private float explodeRadius;

    public bool isExploding = false;

    // Start is called before the first frame update
    void Start()
    {
        explodeRadius = explosionPrefab.GetComponent<Explosion>().explodeRadius;
    }

    public void Explode()
    {   
        isExploding = true;
        
        //this trick will make sure my explosion sfx doesn't mess with Landon's TNT
        if (explosionSfx != null)
        {
            explosionSfx.Play();
        }

        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }

        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        if (Vector3.Distance(transform.position, Player.player.transform.position) < explodeRadius)
        {
            Player.player.TakeDamage(explodeDamage);                    
        }

        Enemy[] enemies = GameManager.gameManager.GetAllEnemies();
        
        for (int i = 0; i < enemies.Length; i++)
        {
            if (Vector3.Distance(transform.position, enemies[i].gameObject.transform.position) < explodeRadius)
            {
                enemies[i].TakeDamage(explodeDamage);
            }
        }

        //eventually we'll want barrels to explode each other
        /*
        ExplodingBarrel[] barrels = GameManager.gameManager.GetAllBarrels();

        for (int i = 0; i < barrels.Length; i++)
        {
            if (Vector3.Distance(transform.position, barrels[i].gameObject.transform.position) < explodeRadius
                && barrels[i].gameObject != this.gameObject)
            {
                barrels[i].GetComponent<ExplodingBarrel>().Explode();
            }
        }
        */
        
        StartCoroutine(WaitToDestroy(2f));
    }

    private IEnumerator WaitToDestroy(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(this.gameObject);
    }
}