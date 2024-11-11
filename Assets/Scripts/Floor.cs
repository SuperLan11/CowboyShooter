using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Floor : MonoBehaviour
{
    [SerializeField] private GameObject singleNavLink;
    [SerializeField] private float maxJumpDist;
    public MeshRenderer myMesh;        
    private List<GameObject> predictedLinks = new List<GameObject>();
    private static int floorsInitialized = 0;
    private int floorLinks = 0;

    // Start is called before the first frame update
    void Start()
    {
        // this script dynamically creates OffMeshLinks based on the size of the floor and the distance to nearby OffMeshLinks
        // this allows NavMesh to jump on platforms from anywhere instead of needing to manually set links
        // if an OffMeshLink goes into a wall, NavMesh will ignore it as long as it has been baked
        // auto update positions needs to be activated in the NavLink prefabs so the start and end transforms can be modified

        myMesh = GetComponent<MeshRenderer>();        
        // how far the NavMesh can jump up/away to another floor
        // make sure the jump distance in the navigation tab is sufficient for this value
        // otherwise NavMesh will not jump even though a link is set
        maxJumpDist = 15f;        
        
        float minLinkX = myMesh.bounds.min.x + 0.5f;
        float maxLinkX = myMesh.bounds.max.x - 0.5f;

        float linkY = myMesh.bounds.max.y + 0.1f;        
        
        float minLinkZ = myMesh.bounds.min.z + 0.5f;
        float maxLinkZ = myMesh.bounds.max.z - 0.5f;

        float curLinkX = myMesh.bounds.min.x + 0.5f;

        int linkNum = 0;

        // place links along top and bottom edges and place a link on nearby floor if it exists
        for (; curLinkX < maxLinkX; curLinkX += 1.0f)
        {
            Vector3 linkPos1 = new Vector3(curLinkX, linkY, minLinkZ);
            GameObject link1 = Instantiate(singleNavLink, linkPos1, Quaternion.identity);
            linkNum++;
            link1.gameObject.name = this.gameObject.name + ".Link" + linkNum;
            predictedLinks.Add(link1);            

            Vector3 offEdge = new Vector3(curLinkX, linkY, minLinkZ - 1f);
            CreateFloorLink(offEdge, link1.GetComponent<OffMeshLink>());            


            Vector3 linkPos2 = new Vector3(curLinkX, linkY, maxLinkZ);
            GameObject link2 = Instantiate(singleNavLink, linkPos2, Quaternion.identity);
            linkNum++;
            link2.gameObject.name = this.gameObject.name + ".Link" + linkNum;
            predictedLinks.Add(link2);
            
            offEdge = new Vector3(curLinkX, linkY, maxLinkZ + 1f);            
            CreateFloorLink(offEdge, link2.GetComponent<OffMeshLink>());            
        }

        float curLinkZ = minLinkZ;                

        // same thing with left and right edges
        for (; curLinkZ < myMesh.bounds.max.z; curLinkZ += 1.0f)
        {
            Vector3 linkPos1 = new Vector3(minLinkX, linkY, curLinkZ);
            GameObject link1 = Instantiate(singleNavLink, linkPos1, Quaternion.identity);
            linkNum++;
            link1.gameObject.name = this.gameObject.name + ".Link" + linkNum;
            predictedLinks.Add(link1);

            Vector3 offEdge = new Vector3(minLinkX - 1f, linkY, curLinkZ);
            CreateFloorLink(offEdge, link1.GetComponent<OffMeshLink>());            


            Vector3 linkPos2 = new Vector3(maxLinkX, linkY, curLinkZ);
            GameObject link2 = Instantiate(singleNavLink, linkPos2, Quaternion.identity);
            linkNum++;
            link2.gameObject.name = this.gameObject.name + ".Link" + linkNum;
            predictedLinks.Add(link2);

            offEdge = new Vector3(maxLinkX + 1f, linkY, curLinkZ);
            CreateFloorLink(offEdge, link2.GetComponent<OffMeshLink>());            
        }
        
        Floor[] floors = FindObjectsOfType<Floor>();
        int numFloors = floors.Length;
        floorsInitialized++;
        // once the last floor has set all its offmeshlinks, the endTransforms of each can be set
        if (floorsInitialized >= numFloors)
        {            
            foreach (Floor floor in floors)
                floor.AssignEndLinks();
        }
    }

    // call this once all links are in
    public void AssignEndLinks()
    {
        // list of bools that determine if the offmeshlink has a valid endTransform nearby
        // if no valid endTransform found, the offmeshlink is destroyed later
        List<bool> doDestroy = new List<bool>();        
        if (predictedLinks.Count > 0)
        {
            for (int i = 0; i < predictedLinks.Count; i++)
            {                
                bool res = SetEnd(predictedLinks[i]);
                doDestroy.Add(!res);
            }
        }
        
        // iterate link list in reverse to prevent issues with indexing after removing
        for(int i = predictedLinks.Count-1; i >= 0; i--)
        {
            if (doDestroy[i])
            {                
                GameObject link = predictedLinks[i];
                predictedLinks.Remove(link);
                Destroy(link);
            }
        }
    }

    // normally the algorithm only places offmeshlinks on the perimeter of a floor
    // this creates floor links on a lower floor if the current floor is on top of another floor
    private void CreateFloorLink(Vector3 topPos, OffMeshLink originalLink)
    {
        RaycastHit hit;
        Vector3 downPos = new Vector3(topPos.x, topPos.y - maxJumpDist, topPos.z);
        // subtract in this order or the raycast goes up!            
        Vector3 direction = (downPos- topPos).normalized;
        if (Physics.Raycast(topPos, direction, out hit, maxJumpDist))
        {            
            if (hit.transform.gameObject.tag == "FLOOR")
            {                
                Vector3 placePos = hit.point;
                placePos.y += 0.1f;
                GameObject link = Instantiate(singleNavLink, placePos, Quaternion.identity);                
                link.GetComponent<OffMeshLink>().endTransform = originalLink.transform;                
                floorLinks++;
                link.name = "FloorLink" + floorLinks;
            }
        }        
    }

    // returns whether the endLink was set successfully. if not, the link will be destroyed
    private bool SetEnd(GameObject newLink)
    {        
        GameObject closestLink = ClosestLink(newLink);                
        MeshRenderer closestFloor = GetClosestFloor(newLink.transform.position);

        /*if (closestLink.transform.position.x > closestFloor.bounds.center.x)
            Debug.Log(closestLink.name + " was on right side of " + closestFloor.gameObject.name);
        if (closestLink.transform.position.x < closestFloor.bounds.center.x)
            Debug.Log(closestLink.name + " was on left side of " + closestFloor.gameObject.name);
        if (closestLink.transform.position.z < closestFloor.bounds.center.z)
            Debug.Log(closestLink.name + " was on near side of " + closestFloor.gameObject.name);
        if (closestLink.transform.position.z > closestFloor.bounds.center.z)
            Debug.Log(closestLink.name + " was on far side of " + closestFloor.gameObject.name);*/

        // to prevent jumping onto a floor from directly beneath the floor
        // check if floor is to left and closest edge is on right or vice versa                
        bool validLeftFloor = (closestLink.transform.position.x < newLink.transform.position.x &&
            closestLink.transform.position.x > closestFloor.bounds.center.x);

        bool validRightFloor = (closestLink.transform.position.x > newLink.transform.position.x &&            
            closestLink.transform.position.x < closestFloor.bounds.center.x);
        
        bool validForwardFloor = (closestLink.transform.position.z > newLink.transform.position.z && 
            closestLink.transform.position.z > closestFloor.bounds.center.z);        

        bool validBackwardFloor = (closestLink.transform.position.z < newLink.transform.position.z && 
            closestLink.transform.position.z < closestFloor.bounds.center.z);       

        bool validFloor = (validLeftFloor || validRightFloor || validForwardFloor || validBackwardFloor);      
        

        float distToLink = Vector3.Distance(newLink.transform.position, closestLink.transform.position);

        if (validFloor && distToLink < maxJumpDist)
        {            
            newLink.GetComponent<OffMeshLink>().endTransform = closestLink.transform;                        
            return true;            
        }        
        else
        {            
            return false; 
        }        
    }

    // should never return null since offmeshlink is only generated on top of a floor
    private MeshRenderer GetClosestFloor(Vector3 closestLink)
    {
        RaycastHit hit;        
        Vector3 downPos = new Vector3(closestLink.x, closestLink.y - 3f, closestLink.z);
        Vector3 direction = (downPos - closestLink).normalized;        
        if (Physics.Raycast(closestLink, direction, out hit, 3f))
        {
            if (hit.transform.gameObject.tag == "FLOOR")            
                return hit.transform.GetComponent<Floor>().myMesh;            
            Debug.LogWarning("hit something, returning null");
            return null;
        }
        Debug.LogWarning("no hit, returning null");
        return null;        
    }

    // returns the closest link to the given link
    GameObject ClosestLink(GameObject newLink)
    {
        OffMeshLink[] links = FindObjectsOfType<OffMeshLink>();        
        List<OffMeshLink> linkList = new List<OffMeshLink>();
        foreach (OffMeshLink link in links)
        {                        
            // don't connect offmeshlinks on the same floor!
            bool diffFloor = !(predictedLinks.Contains(link.gameObject));
            if (diffFloor)                                
                linkList.Add(link);
        }

        if (linkList.Count == 0)
            return null;

        float dist;
        float minDist = Mathf.Infinity;
        int closestIndex = 0;
        for (int i = 0; i < linkList.Count; i++)
        {
            dist = Vector3.Distance(newLink.transform.position, linkList[i].transform.position);
            if(dist < minDist)
            {
                minDist = dist;
                closestIndex = i;
            }
        }

        return linkList[closestIndex].gameObject;
    }   

    // Update is called once per frame
    void Update()
    {
        
    }
}