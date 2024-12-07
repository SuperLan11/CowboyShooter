using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigTornado : MonoBehaviour
{
    [SerializeField] private Transform stopLoc;
    [SerializeField] private float tornadoAccel;
    public static float tornadoTopY;
    public static float spinSpeed = 100f;
    public static float spinAccel = 0.3f;

    private AudioSource tornadoSfx;

    // Start is called before the first frame update
    void Start()
    {
        tornadoTopY = GetComponent<MeshRenderer>().bounds.max.y;
        tornadoSfx = GetComponent<AudioSource>();
        if (tornadoSfx != null)
            tornadoSfx.Play();
    }

    private void OnCollisionEnter(Collision collision)
    {
        for(int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).otherCollider.gameObject.name == "Player")
            {
                Player.player.currentMovementState = Player.movementState.TORNADO;                
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("dist to stop: " + Vector3.Distance(transform.position, stopLoc.position));        
        transform.position = Vector3.Lerp(transform.position, stopLoc.position, tornadoAccel*Time.deltaTime);
    }
}
