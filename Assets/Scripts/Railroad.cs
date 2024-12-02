using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Railroad : MonoBehaviour
{
    [SerializeField] protected float rrSpeed;
    public static float railroadSpeed;
    [SerializeField] protected float xMax;
    public static float maxX;

    [SerializeField] private GameObject tunnelPrefab;
    private float railroadLength;

    // works with multiple railroads since tile spawns are set manually for each column/row
    [SerializeField] protected Transform initSpawnTile;
    protected Vector3 spawnPos;

    [SerializeField] protected float tunnelCooldown;
    // how far tunnel will spawn from the player
    [SerializeField] protected float tunnelOffset;            
    private float tunnelLength;

    private void Awake()
    {
        // static variables can't be serialized
        // set the static vars to the serialized values before any start function executes
        railroadSpeed = rrSpeed;
        maxX = xMax;
    }

    // Start is called before the first frame update
    void Start()
    {
        AssignSpawn();
        tunnelLength = tunnelPrefab.GetComponent<MeshRenderer>().bounds.size.x;
        StartCoroutine(TunnelCooldown(tunnelCooldown));
        railroadLength = GetComponent<BoxCollider>().bounds.size.x;
    }

    protected void AssignSpawn()
    {
        if (initSpawnTile != null)
            spawnPos = initSpawnTile.position;
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
        Vector3 newPos = transform.position;
        newPos.x += railroadSpeed * Time.deltaTime;
        transform.position = newPos;                

        if (transform.position.x > maxX)
        {            
            transform.position = spawnPos;
            /*if(hasTunnel)
            {
                Destroy(transform.GetChild(0).gameObject);
                hasTunnel = false;
            }*/
        }        
    }
}
