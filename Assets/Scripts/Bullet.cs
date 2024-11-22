using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float maxTimeAlive;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TTL(maxTimeAlive));
    }

    private IEnumerator TTL(float maxTime)
    {
        yield return new WaitForSecondsRealtime(maxTime);
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider collider)
    {        
        bool hitPlayer = collider.gameObject.name == "Player";
        bool hitWall = collider.gameObject.tag == "WALL";
        bool hitFloor = collider.gameObject.tag == "FLOOR";
        if (hitPlayer)
        {
            Player.player.TakeDamage(1);
            Destroy(this.gameObject);            
        }
        else if (hitWall || hitFloor)
        {
            Destroy(this.gameObject);            
        }        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
