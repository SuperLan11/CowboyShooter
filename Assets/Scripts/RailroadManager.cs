using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailroadManager : MonoBehaviour
{
    [SerializeField] protected float rrSpeed;
    public static float railroadSpeed;
    [SerializeField] protected float xMax;
    public static float maxX;
    [SerializeField] protected float topRailroadY;
    public static float railroadTopY;

    [SerializeField] private GameObject tunnelPrefab;    

    [SerializeField] protected float tunnelCooldown;
    // how far tunnel will spawn from the player
    [SerializeField] protected float tunnelOffset;
    private float tunnelLength;

    private List<Transform> movingTiles = new List<Transform>();    

    public static GameObject[] decorations;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject skullPrefab;
    [SerializeField] private GameObject canyonPrefab;
    [SerializeField] private GameObject shortCactusPrefab;
    [SerializeField] private GameObject tallCactusPrefab;

    private List<float> railroadZs = new List<float>();

    private float railroadLength;
    private bool isRailroad;

    private void Awake()
    {
        // static variables can't be serialized
        // set the static vars to the serialized values before any start function executes
        railroadSpeed = rrSpeed;
        maxX = xMax;
        railroadTopY = topRailroadY;
        decorations = new GameObject[] { rockPrefab, skullPrefab, canyonPrefab, shortCactusPrefab, tallCactusPrefab };
    }

    // Start is called before the first frame update
    void Start()
    {
        railroadLength = Railroad.railroadLength;
        tunnelLength = tunnelPrefab.GetComponent<MeshRenderer>().bounds.size.x;
        StartCoroutine(TunnelCooldown(tunnelCooldown));

        Railroad[] railroads = FindObjectsOfType<Railroad>();
        RailroadTile[] railroadTiles = FindObjectsOfType<RailroadTile>();
        foreach(Railroad railroad in railroads)
        {
            movingTiles.Add(railroad.transform);
            if (!railroadZs.Contains(railroad.transform.position.z))
                railroadZs.Add(railroad.transform.position.z);
        }        

        foreach (RailroadTile railroadTile in railroadTiles)
        {
            if(!movingTiles.Contains(railroadTile.transform))
                movingTiles.Add(railroadTile.transform);
        }                
    }

    private IEnumerator TunnelCooldown(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);                    
        
        // this can spawn tunnels on multiple railroads at once since xDiff is used instead of distance
        foreach (float railroadZ in railroadZs)
        {          
            // to prevent errors when tunnel countdowns finishes while on main menu
            if (Player.player == null)
                break;

            // fix this later, need z of each railroad
            Vector3 tunnelPos;
            float xDiff = Player.player.transform.position.x - transform.position.x;
            tunnelPos.x = Player.player.transform.position.x - xDiff;
            tunnelPos.y = railroadTopY;
            tunnelPos.z = railroadZ;
            Instantiate(tunnelPrefab, tunnelPos, tunnelPrefab.transform.rotation);
        }
        // recursively call coroutine for every railroad
        StartCoroutine(TunnelCooldown(tunnelCooldown));
    }

    private void SetLastTile(Transform prevLastTile, Transform newLastTile)
    {
        // better solution would be to get all railroads and railroad tiles in start and reference that
        // instead of finding the railroads and railroad tiles every time you call this function
        foreach(Railroad railroad in FindObjectsOfType<Railroad>())
        {
            if (railroad.lastTile[0] == prevLastTile)
                railroad.lastTile[0] = newLastTile;            
        }

        foreach (RailroadTile railroadTile in FindObjectsOfType<RailroadTile>())
        {
            if (railroadTile.lastTile[0] == prevLastTile)
                railroadTile.lastTile[0] = newLastTile;
        }
    }

    private void CycleTile(GameObject tile)
    {
        float lastTileLength;
        Vector3 newTilePos;
        Vector3 lastTilePos;
        if (tile.GetComponent<RailroadTile>() != null)
        {
            lastTileLength = tile.GetComponent<RailroadTile>().lastTile[0].gameObject.GetComponent<MeshRenderer>().bounds.size.x;
            newTilePos = tile.GetComponent<RailroadTile>().lastTile[0].gameObject.GetComponent<MeshRenderer>().bounds.center;
            //newTilePos = tile.GetComponent<RailroadTile>().lastTile[0].transform.position;

            lastTilePos = tile.GetComponent<RailroadTile>().lastTile[0].position;
        }
        else // tile.GetComponent<Railroad>() != null
        {
            lastTileLength = tile.GetComponent<Railroad>().lastTile[0].gameObject.GetComponent<MeshRenderer>().bounds.size.x;
            newTilePos = tile.GetComponent<Railroad>().lastTile[0].gameObject.GetComponent<MeshRenderer>().bounds.center;
            //newTilePos = tile.GetComponent<Railroad>().lastTile[0].transform.position;

            lastTilePos = tile.GetComponent<Railroad>().lastTile[0].position;
        }

        Debug.Log("tile transform.pos before cycling: " + tile.transform.position);
        float distPastEnd = (tile.transform.position.x - RailroadManager.maxX);
        newTilePos.x -= lastTileLength;
        //newTilePos.x += 2*distPastEnd;        
        //Debug.Log("lastTileLength: " + lastTileLength + ", newTilePos: " + newTilePos);
        tile.transform.position = newTilePos;
        
        Debug.Log("last tile length: " + lastTileLength + ", dist between: " + Vector3.Distance(tile.transform.position, lastTilePos));
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Transform tile in movingTiles)
        {
            Vector3 newPos = tile.transform.position;
            newPos.x += railroadSpeed * Time.deltaTime;
            tile.transform.position = newPos;

            if (tile.transform.position.x > maxX)
            {
                if (tile.GetComponent<RailroadTile>() != null)
                {
                    //tile.transform.position = tile.GetComponent<RailroadTile>().spawnPos;
                    CycleTile(tile.gameObject);
                    SetLastTile(tile.GetComponent<RailroadTile>().lastTile[0], tile.transform);
                }
                else if (tile.GetComponent<Railroad>() != null)
                {
                    //tile.transform.position = tile.GetComponent<Railroad>().spawnPos;
                    CycleTile(tile.gameObject);
                    SetLastTile(tile.GetComponent<Railroad>().lastTile[0], tile.transform);
                }
            }            
        }
    }
}
