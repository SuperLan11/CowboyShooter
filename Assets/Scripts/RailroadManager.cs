using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailroadManager : MonoBehaviour
{
    [SerializeField] protected float rrSpeed;
    public static float railroadSpeed;
    [SerializeField] protected float xMax;
    public static float maxX;

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

    private float railroadLength;
    private bool isRailroad;

    private void Awake()
    {
        // static variables can't be serialized
        // set the static vars to the serialized values before any start function executes
        railroadSpeed = rrSpeed;
        maxX = xMax;
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
        }

        foreach (RailroadTile railroadTile in railroadTiles)
        {
            if(!movingTiles.Contains(railroadTile.transform))
                movingTiles.Add(railroadTile.transform);
        }        

        /*
         * railroad and railroadtile scripts will serialize initSpawnTile, manager does the rest        
         * get 2d array of all moving tiles, do normal update foreach moving tile, offset by first list tile transform
         * OR get list of spawnPositions and set position to corresponding position index
         */
    }

    private IEnumerator TunnelCooldown(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);

        float xDiff = Player.player.transform.position.x - transform.position.x;
        // this checks if the tile is within a certain xDistance from the player.
        // one railroad is guaranteed to be this distance from the player since the
        // range is as long as a railroad.
        // this can spawn tunnels on multiple railroads at once since xDiff is used instead of distance
                
        if (xDiff > (tunnelLength + tunnelOffset) && xDiff < (tunnelLength + tunnelOffset + railroadLength))
        {
            Vector3 tunnelPos;
            tunnelPos.x = Player.player.transform.position.x - xDiff;
            tunnelPos.y = GetComponent<MeshRenderer>().bounds.max.y;
            tunnelPos.z = transform.position.z;
            Instantiate(tunnelPrefab, tunnelPos, tunnelPrefab.transform.rotation);
        }
        // recursively call coroutine for every railroad
        StartCoroutine(TunnelCooldown(tunnelCooldown));
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
                    tile.transform.position = tile.GetComponent<RailroadTile>().spawnPos;                    
                }
                else if (tile.GetComponent<Railroad>() != null)
                {
                    tile.transform.position = tile.GetComponent<Railroad>().spawnPos;
                }                
            }            
        }
    }
}
