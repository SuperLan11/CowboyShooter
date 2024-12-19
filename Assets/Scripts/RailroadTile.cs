using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailroadTile : MonoBehaviour
{    
    [SerializeField] private int maxDecorations = 10;
    [System.NonSerialized] public Vector3 spawnPos;
    [SerializeField] protected Transform initSpawnTile;
    // use an array with only the last tile transform in it so you can assign the transform of the array
    // instead of assigning the tile transform to another tile's transform
    [System.NonSerialized] public Transform[] lastTile = new Transform[1];

    private void Start()
    {
        if (initSpawnTile != null)
            spawnPos = initSpawnTile.position;

        lastTile[0] = initSpawnTile;

        // ideally make the railroad tiles very big to minimize the frequency of randomization (if decorations are re-randomized)
        RandomizeDeco();
    }
    
    private void DestroyAnyDecos()
    {
        for (int i = 0; i < transform.childCount; i++)        
            Destroy(transform.GetChild(i).gameObject); 
    }    
    
    private bool IsObjectHere(GameObject newDeco, Vector3 potentialPos)
    {
        Collider[] collisions = Physics.OverlapSphere(potentialPos, newDeco.GetComponent<MeshRenderer>().bounds.size.x / 2);
        foreach (Collider collider in collisions)
        {
            bool isRailroad = collider.gameObject.GetComponent<Railroad>() != null;
            bool isRailroadTile = collider.gameObject.GetComponent<RailroadTile>() != null;
            bool isDeathZone = collider.gameObject.GetComponent<DeathZone>() != null;
            if (!isRailroad && !isRailroadTile && !isDeathZone)          
                return true;            
        }       
        return false;
    }

    private void RandomizeDeco()
    {
        int decosLeft = maxDecorations;        
        // to prevent infinite loop if can't fit any new decorations
        int numIters = 0;        

        while (decosLeft > 0 && numIters < 100)
        {            
                // max is exclusive
                int randIdx = Random.Range(0, RailroadManager.decorations.Length);                      
                GameObject potentialDeco = RailroadManager.decorations[randIdx];

                float scale = 1.0f;
                if (potentialDeco.name.Contains("Canyone"))
                {
                    while (scale * potentialDeco.GetComponent<MeshRenderer>().bounds.size.z > GetComponent<MeshRenderer>().bounds.size.z)
                        scale = Random.Range(0, 10) * 0.1f;
                }

                float decoHalfXSize = scale * potentialDeco.GetComponent<MeshRenderer>().bounds.size.x / 2;
                float decoHalfZSize = scale * potentialDeco.GetComponent<MeshRenderer>().bounds.size.z / 2;

                float randX = Random.Range(GetComponent<Collider>().bounds.min.x + decoHalfXSize, GetComponent<Collider>().bounds.max.x - decoHalfXSize);
                float randZ = Random.Range(GetComponent<Collider>().bounds.min.z + decoHalfZSize, GetComponent<Collider>().bounds.max.z - decoHalfZSize);
                float y = GetComponent<BoxCollider>().bounds.max.y;
                Vector3 potentialPos = new Vector3(randX, y, randZ);                                  

                if (!IsObjectHere(potentialDeco, potentialPos))
                {
                    GameObject newDeco = Instantiate(potentialDeco, potentialPos, potentialDeco.transform.rotation);
                    decosLeft--;
                    if (scale != 1.0f)                    
                        newDeco.transform.localScale *= scale;
                    newDeco.transform.SetParent(this.transform);          
                }
                numIters++;
        }
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
    
    private IEnumerator WaitToRedeco(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        RandomizeDeco();
    }

  /*  private void CycleTiles()
    {
        for (int i = 0; i < railroads.Length - 1; i++)
        {
            if (railroads[i] != null)
                railroads[i + 1] = railroads[i];
        }
        if (railroads[railroads.Length - 1] != null)
        {
            railroads[0] = railroads[railroads.Length - 1];
            *//*Vector3 newPos;
            railroads[0].position = railroads[1].position*//*
        }

        for (int i = 0; i < railroads.Length; i++)
        {
            Debug.Log("cycled railroads[" + i + "]: " + railroads[i].position);
        }
    }   */ 

    /*
     * algorithm to randomize decorations:     
     * On Start():
     * get a randomized object in the array, get a random position and subtract the object size from bounds
     * if the bounds of the randomized object would fit within the tile's x, z bounds without colliding another placed object
     * and the number of objects on the tile is less than 10, instantiate the object on the tile in that position
     * parent the new object to the tile
     * randomize decorations when restarting cycle
     */
}
