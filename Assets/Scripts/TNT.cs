using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNT : MonoBehaviour
{
    [SerializeField] private float timeToExplode;
    [SerializeField] private AudioSource sizzleSfx;
    private AudioSource boomSfx;

    // Start is called before the first frame update
    void Start()
    {
        boomSfx = FindAnyObjectByType<ThrowEnemy>().boomSfx;
        if(sizzleSfx != null)
            sizzleSfx.Play();

        StartCoroutine(TTL(timeToExplode));
    }

    private IEnumerator TTL(float seconds)
    {
        yield return new WaitForSeconds(seconds);        

        if (sizzleSfx != null && sizzleSfx.isPlaying)
            sizzleSfx.Stop();
        // should continue playing after destroying tnt since enemy has AudioSource
        if (boomSfx != null)
            boomSfx.Play();
        
        Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
