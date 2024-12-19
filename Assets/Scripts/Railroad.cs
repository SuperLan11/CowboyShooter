using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Railroad : MonoBehaviour
{       
    // works with multiple railroads since tile spawns are set manually for each column/row
    [SerializeField] protected Transform initSpawnTile;
    public static float railroadLength;
    [System.NonSerialized] public Vector3 spawnPos;
    // use an array with only the last tile transform in it so you can assign the transform of the array
    // instead of assigning the tile transform to another tile's transform
    [System.NonSerialized] public Transform[] lastTile = new Transform[1];

    private void Awake()
    {
        railroadLength = GetComponent<BoxCollider>().bounds.size.x;
    }

    // Start is called before the first frame update
    void Start()
    {        
        if (initSpawnTile != null)
            spawnPos = initSpawnTile.position;
        
        lastTile[0] = initSpawnTile;
    }

    private void OnCollisionEnter(Collision collision)
    {       
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.gameObject.name == "Player")
            {
                Player.player.TakeDamage(Player.player.GetHealth());
                break;
            }
        }
    }        
}
