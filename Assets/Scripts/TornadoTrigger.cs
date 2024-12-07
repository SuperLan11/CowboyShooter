using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoTrigger : MonoBehaviour
{
    [SerializeField] private GameObject tornadoPrefab;
    //[SerializeField] private GameObject smallTornadoPrefab;
    [SerializeField] private Transform tornadoSpawn;
    private bool tornadoPlaced = false;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (tornadoPlaced)
            return;
        
        if(collider.gameObject.name == "Player")
        {
            Debug.Log("made tornado");
            //Instantiate(tornadoPrefab, tornadoSpawn.position, Quaternion.identity);
            Instantiate(tornadoPrefab, tornadoSpawn.position, Quaternion.identity);
            tornadoPlaced = true;
        }                
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
