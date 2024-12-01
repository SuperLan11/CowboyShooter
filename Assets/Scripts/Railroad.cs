using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Railroad : MonoBehaviour
{
    [SerializeField] protected float railroadSpeed;
    [SerializeField] protected float maxX;
    [SerializeField] private GameObject tunnelPrefab;
    private float railroadLength;

    // works with multiple railroads since tile spawns are set manually for each column/row
    [SerializeField] protected Transform initSpawnTile;
    protected Vector3 spawnPos;

    [SerializeField] protected float tunnelCooldown;
    [SerializeField] protected float tunnelOffset;
    private static bool tunnelCooldownDone = false;
    private bool hasTunnel = false;

    // how to create one tunnel or multiple tunnels for each railroad? center railroad?
    private float xBetweenTracks;

    // Start is called before the first frame update
    void Start()
    {
        AssignSpawn();
        StartCoroutine(TunnelCooldown(tunnelCooldown));
        railroadLength = GetComponent<BoxCollider>().bounds.size.x;
    }

    private IEnumerator TunnelCooldown(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        tunnelCooldownDone = true;
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

    // Update is called once per frame
    void Update()
    {        
        Vector3 newPos = transform.position;
        newPos.x += railroadSpeed * Time.deltaTime;
        transform.position = newPos;

        if (transform.position.x > maxX)
        {            
            transform.position = spawnPos;
            if(hasTunnel)
            {
                Destroy(transform.GetChild(0).gameObject);
                hasTunnel = false;
            }
            // only spawn tunnels at the end of the railroad
            /*else if(tunnelCooldownDone)
            {                
                tunnelCooldownDone = false;
                StartCoroutine(TunnelCooldown(tunnelCooldown));                
                // how to spawn tunnels on both sides or one tunnel for each side
                GameObject tunnel = Instantiate(tunnelPrefab, spawnPos, tunnelPrefab.transform.rotation);
                tunnel.transform.SetParent(this.transform);
                hasTunnel = true;
            }*/
        }
        float xDiff = transform.position.x - Player.player.transform.position.x;
        if (tunnelCooldownDone && xDiff > tunnelOffset && xDiff < (tunnelOffset + railroadLength))
        {
            tunnelCooldownDone = false;
            StartCoroutine(TunnelCooldown(tunnelCooldown));
            // how to spawn tunnels on both sides or one tunnel for each side
            GameObject tunnel = Instantiate(tunnelPrefab, spawnPos, tunnelPrefab.transform.rotation);
            tunnel.transform.SetParent(this.transform);
            hasTunnel = true;
        }
    }
}
