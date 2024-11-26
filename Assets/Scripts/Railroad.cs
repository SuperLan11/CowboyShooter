using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Railroad : MonoBehaviour
{
    [SerializeField] protected float railroadSpeed;
    [SerializeField] protected float maxX;    

    [SerializeField] protected Transform initSpawnTile;
    protected Vector3 spawnPos;

    // Start is called before the first frame update
    void Start()
    {
        AssignSpawn();
    }

    protected void AssignSpawn()
    {
        if (initSpawnTile != null)
            spawnPos = initSpawnTile.position;
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
