using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Railroad : MonoBehaviour
{
    [SerializeField] private float railroadSpeed;
    [SerializeField] private float maxX;
    private static float railroadLength;

    [SerializeField] private Transform initSpawnTile;
    private Vector3 spawnPos;
    [SerializeField] private int railroadNum;

    /* make it so you only need to input last tile and railroad num */

    // Start is called before the first frame update
    void Start()
    {
        if (initSpawnTile != null)
            spawnPos = initSpawnTile.position;        

        railroadLength = GetComponent<BoxCollider>().bounds.size.x;
    }

    // Update is called once per frame
    void Update()
    {        
        Vector3 newPos = transform.position;
        newPos.x += railroadSpeed * Time.deltaTime;
        transform.position = newPos;

        if (transform.position.x > maxX)                            
            transform.position = spawnPos;        
    }
}
