using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailroadTile : Railroad
{
    private GameObject[] decorations;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject skullPrefab;
    [SerializeField] private GameObject canyonPrefab;
    [SerializeField] private GameObject shortCactusPrefab;
    [SerializeField] private GameObject tallCactusPrefab;
    // has weird scale issues
    //[SerializeField] private GameObject flowerCactusPrefab;

    [SerializeField] private int maxDecorations = 10;
    private List<GameObject> curDecorations = new List<GameObject>();
    private BoxCollider boxCollider;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        AssignSpawn();
        decorations = new GameObject[] { rockPrefab, skullPrefab, canyonPrefab, shortCactusPrefab, tallCactusPrefab };
        // ideally make the railroad tiles very big to minimize the frequency of randomization
        RandomizeDeco();
    }

    // not working
    private bool IsObjectHere(GameObject[] decosPlaced, GameObject newDeco, Vector3 potentialPos)
    {
        Collider[] collisions = Physics.OverlapSphere(potentialPos, newDeco.GetComponent<MeshRenderer>().bounds.size.x / 2);
        foreach (Collider collider in collisions)
        {
            bool isRailroad = collider.gameObject.GetComponent<Railroad>() != null;
            bool isRailroadTile = collider.gameObject.GetComponent<RailroadTile>() != null;
            if (!isRailroad && !isRailroadTile)                
                return true;
        }
        return false;
    }

    // has errors
    private void DestroyAnyDecos()
    {
        foreach (GameObject deco in curDecorations)
        {
            curDecorations.Remove(deco);
            Destroy(deco);
        }
    }

    private bool IsUniqueDeco(GameObject newDeco)
    {
        int canyons = 0;
        int tallCacti = 0;
        int shortCatci = 0;
        int flowerCacti = 0;
        int skulls = 0;

        if (newDeco.name.Contains(canyonPrefab.name))
            canyons++;
        if (newDeco.name.Contains(tallCactusPrefab.name))
            tallCacti++;
        if (newDeco.name.Contains(shortCactusPrefab.name))
            shortCatci++;
        /*if (newDeco.name.Contains(flowerCactusPrefab.name))
            flowerCacti++;*/
        if (newDeco.name.Contains(skullPrefab.name))
            skulls++;

        foreach (GameObject obj in curDecorations)
        {
            if (obj.name.Contains(canyonPrefab.name))
                canyons++;
            if (obj.name.Contains(tallCactusPrefab.name))
                tallCacti++;
            if (obj.name.Contains(shortCactusPrefab.name))
                shortCatci++;
            /*if (obj.name.Contains(flowerCactusPrefab.name))
                flowerCacti++;*/
            if (obj.name.Contains(skullPrefab.name))
                skulls++;

            if (canyons > 1 || tallCacti > 1 || shortCatci > 1 || flowerCacti > 1 || skulls > 1)
                return false;
        }
        return true;
    }

    private void RandomizeDeco()
    {
        //DestroyAnyDecos();

        int itemsLeft = maxDecorations;
        // this isn't really used at the moment
        bool canPlace = true;
        
        GameObject[] decosPlaced = new GameObject[decorations.Length];

        while (itemsLeft > 0 && canPlace)
        {
            // max is exclusive
            int randIdx = Random.Range(0, decorations.Length);
            GameObject potentialDeco = decorations[randIdx];

            float scale = 1.0f;

            if(potentialDeco.name.Contains("Canyone"))
            {
                scale = Random.Range(0, 10) * 0.1f;
            }
            int numIters = 0;

            float decoHalfXSize = scale * potentialDeco.GetComponent<MeshRenderer>().bounds.size.x / 2;
            float decoHalfZSize = scale * potentialDeco.GetComponent<MeshRenderer>().bounds.size.z / 2;

            float randX = Random.Range(GetComponent<Collider>().bounds.min.x + decoHalfXSize, GetComponent<Collider>().bounds.max.x - decoHalfXSize);
            float randZ = Random.Range(GetComponent<Collider>().bounds.min.z + decoHalfZSize, GetComponent<Collider>().bounds.max.z - decoHalfZSize);                       
            float y = GetComponent<BoxCollider>().bounds.max.y;
            Vector3 potentialPos = new Vector3(randX, y, randZ);                        

            if (IsObjectHere(decosPlaced, potentialDeco, potentialPos))
            {
                continue;
            }
            // prevent looping forever if no more decorations can fit
            else if(numIters < 100)
            {
                itemsLeft--;
                GameObject newDeco = Instantiate(potentialDeco, potentialPos, potentialDeco.transform.rotation);
                if (scale != 1.0f)
                {
                    float xScale = newDeco.transform.localScale.x;
                    float yScale = newDeco.transform.localScale.y;
                    float zScale = newDeco.transform.localScale.z;
                    newDeco.transform.localScale = new Vector3(xScale * scale, yScale * scale, zScale * scale);                    
                }
                newDeco.transform.SetParent(this.transform);
                curDecorations.Add(newDeco);
            }
            else
            {
                break;
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

    void Update()
    {
        Vector3 newPos = transform.position;
        newPos.x += railroadSpeed * Time.deltaTime;
        transform.position = newPos;

        if (transform.position.x > maxX)
        {
            transform.position = spawnPos;
            // uncomment once destroying deco works without errors
            //RandomizeDeco();
        }
    }

    /*
     * algorithm to randomize decorations:     
     * On Start():
     * get a randomized object in the array, get a random position and subtract the object size from bounds
     * if the bounds of the randomized object would fit within the tile's x, z bounds without colliding another placed object
     * the number of objects on the tile is less than 10, and the object is unique on the tile, instantiate the object on the tile in that position
     * parent the new object to the tile
     * randomize decorations when restarting cycle
     */
}
