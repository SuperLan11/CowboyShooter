using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{    
    [SerializeField] public float explodeRadius = 2f;
    // only change if fade animation changes length
    [SerializeField] public static float fadeTime = 1f;

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale *= (2*explodeRadius);        
        StartCoroutine(TTL(fadeTime));
    }

    private IEnumerator TTL(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
