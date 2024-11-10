using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Floor : MonoBehaviour
{
    [SerializeField] private GameObject navLinkPrefab;
    private MeshRenderer myMesh;    

    // Start is called before the first frame update
    void Start()
    {
        myMesh = GetComponent<MeshRenderer>();                
        
        float minLinkX = myMesh.bounds.min.x + 0.5f;
        float maxLinkX = myMesh.bounds.max.x - 0.5f;

        float highLinkY = myMesh.bounds.max.y + 0.1f;
        float lowLinkY = myMesh.bounds.min.y + 0.15f;
        
        float minLinkZ = myMesh.bounds.min.z + 0.5f;
        float maxLinkZ = myMesh.bounds.max.z - 0.5f;

        float curLinkX = myMesh.bounds.min.x + 0.5f;        

        // place links along top and bottom edges
        for (; curLinkX < maxLinkX; curLinkX += 1.0f)
        {
            Vector3 linkPos1 = new Vector3(curLinkX, highLinkY, minLinkZ);
            GameObject link1 = Instantiate(navLinkPrefab, linkPos1, Quaternion.identity);
            link1.GetComponent<OffMeshLink>().endTransform.position = new Vector3(curLinkX, lowLinkY, minLinkZ - 1.0f);            

            Vector3 linkPos2 = new Vector3(curLinkX, highLinkY, maxLinkZ);
            GameObject link2 = Instantiate(navLinkPrefab, linkPos2, Quaternion.identity);
            link2.GetComponent<OffMeshLink>().endTransform.position = new Vector3(curLinkX, lowLinkY, maxLinkZ + 1.0f);
        }

        float curLinkZ = minLinkZ;                

        // place links along left and right edges
        for (; curLinkZ < myMesh.bounds.max.z; curLinkZ += 1.0f)
        {
            Vector3 linkPos1 = new Vector3(curLinkX, highLinkY, curLinkZ);
            GameObject link1 = Instantiate(navLinkPrefab, linkPos1, Quaternion.identity);
            link1.GetComponent<OffMeshLink>().endTransform.position = new Vector3(minLinkX - 1.0f, lowLinkY, curLinkZ);            

            Vector3 linkPos2 = new Vector3(curLinkX, highLinkY, curLinkZ);
            GameObject link2 = Instantiate(navLinkPrefab, linkPos2, Quaternion.identity);
            link2.GetComponent<OffMeshLink>().endTransform.position = new Vector3(maxLinkX + 1.0f, lowLinkY, curLinkZ);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
