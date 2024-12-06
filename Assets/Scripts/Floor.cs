//With floor link issue


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Floor : MonoBehaviour
{
    [SerializeField] private GameObject singleNavLink;
    // how far the NavMesh can jump up/away to another floor
    // make sure the jump distance in the navigation tab is sufficient for this value
    // otherwise NavMesh will not jump even though a link is set
    private float maxJumpDist = Enemy.maxJumpDist;

    private MeshRenderer myMesh;
    private List<GameObject> perimiterLinks = new List<GameObject>();

    public static int floorsInitialized = 0;
    private int numFloorLinks = 0;

    private static List<Vector3> floorLinks = new List<Vector3>();

    private List<Vector3> completedLinks = new List<Vector3>();
    [SerializeField] private bool useUniqueLinks = true;

    // incrasing space per link decreases the density of offmeshlinks, which helps performance
    [SerializeField] private float xSpacePerLink;
    [SerializeField] private float zSpacePerLink;

    // Start is called before the first frame update
    void Start()
    {
        // this script dynamically creates OffMeshLinks based on the size of the floor and the distance to nearby OffMeshLinks
        // this allows NavMesh to jump on platforms from anywhere instead of needing to manually set links
        // if an OffMeshLink goes into a wall, NavMesh will ignore it as long as it has been baked
        // auto update positions needs to be activated in the NavLink prefabs so the start and end transforms can be modified        

        myMesh = GetComponent<MeshRenderer>();
        
        float minLinkX = myMesh.bounds.min.x + xSpacePerLink;
        float maxLinkX = myMesh.bounds.max.x - xSpacePerLink;

        float linkY = myMesh.bounds.max.y + 0.1f;

        float minLinkZ = myMesh.bounds.min.z + zSpacePerLink;
        float maxLinkZ = myMesh.bounds.max.z - zSpacePerLink;

        float leftEdge = myMesh.bounds.min.x;
        float rightEdge = myMesh.bounds.max.x;

        float frontEdge = myMesh.bounds.max.z;
        float backEdge = myMesh.bounds.min.z;

        float curLinkX = minLinkX;

        int linkNum = 0;        

        // place links along top and bottom edges and place a link on nearby floor if it exists
        for (; curLinkX < maxLinkX; curLinkX += xSpacePerLink)
        {
            Vector3 linkPos1 = new Vector3(curLinkX, linkY, minLinkZ);
            GameObject link1 = Instantiate(singleNavLink, linkPos1, Quaternion.identity);
            linkNum++;
            link1.gameObject.name = this.gameObject.name + ".Link" + linkNum;            
            perimiterLinks.Add(link1);            

            Vector3 offEdge = new Vector3(curLinkX, linkY, backEdge - 1f);
            CreateFloorLink(offEdge, link1.GetComponent<OffMeshLink>());


            Vector3 linkPos2 = new Vector3(curLinkX, linkY, maxLinkZ);
            GameObject link2 = Instantiate(singleNavLink, linkPos2, Quaternion.identity);
            linkNum++;
            link2.gameObject.name = this.gameObject.name + ".Link" + linkNum;            
            perimiterLinks.Add(link2);            

            offEdge = new Vector3(curLinkX, linkY, frontEdge + 1f);
            CreateFloorLink(offEdge, link2.GetComponent<OffMeshLink>());
        }

        float curLinkZ = minLinkZ;

        // same thing with left and right edges
        for (; curLinkZ < myMesh.bounds.max.z; curLinkZ += zSpacePerLink)
        {
            Vector3 linkPos1 = new Vector3(minLinkX, linkY, curLinkZ);
            GameObject link1 = Instantiate(singleNavLink, linkPos1, Quaternion.identity);
            linkNum++;
            link1.gameObject.name = this.gameObject.name + ".Link" + linkNum;            
            perimiterLinks.Add(link1);            

            Vector3 offEdge = new Vector3(leftEdge - 1f, linkY, curLinkZ);
            CreateFloorLink(offEdge, link1.GetComponent<OffMeshLink>());


            Vector3 linkPos2 = new Vector3(maxLinkX, linkY, curLinkZ);
            GameObject link2 = Instantiate(singleNavLink, linkPos2, Quaternion.identity);
            linkNum++;
            link2.gameObject.name = this.gameObject.name + ".Link" + linkNum;                        
            perimiterLinks.Add(link2);            

            offEdge = new Vector3(rightEdge + 1f, linkY, curLinkZ);
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
        if (perimiterLinks.Count == 0)
            return;

        // list of bools that determine if the offmeshlink has a valid endTransform nearby
        // if no valid endTransform found, the offmeshlink is destroyed later
        List<bool> doDestroy = new List<bool>();
        for (int i = 0; i < perimiterLinks.Count; i++)
        {            
            // predictedLinks contains all the perimter links for the floor
            bool endSet = SetEnd(perimiterLinks[i]);
            doDestroy.Add(!endSet);
        }
        
        // iterate link list in reverse to prevent issues with indexing after removing
        for (int i = perimiterLinks.Count - 1; i >= 0; i--)
        {
            if (doDestroy[i])
            {
                GameObject link = perimiterLinks[i];
                perimiterLinks.Remove(link);
                                
                Destroy(link);
            }
        }        
    }

    // normally the algorithm only places offmeshlinks on the perimeter of a floor
    // this creates floor links on a lower floor if the current floor is on top of another floor
    private bool CreateFloorLink(Vector3 topPos, OffMeshLink originalLink)
    {
        RaycastHit hit;
        Vector3 downPos = new Vector3(topPos.x, topPos.y - maxJumpDist, topPos.z);
        // subtract in this order or the raycast goes up!            
        Vector3 direction = (downPos - topPos).normalized;
        if (Physics.Raycast(topPos, direction, out hit, maxJumpDist))
        {
            if (hit.transform.gameObject.tag == "FLOOR")
            {
                Vector3 placePos = hit.point;
                placePos.y += 0.1f;
                GameObject floorLink = Instantiate(singleNavLink, placePos, Quaternion.identity);
                originalLink.GetComponent<OffMeshLink>().endTransform = floorLink.transform;

                // don't set endTransform for top floor links twice
                perimiterLinks.Remove(originalLink.gameObject);

                Floor.floorLinks.Add(floorLink.transform.position);
                Floor.floorLinks.Add(originalLink.transform.position);                

                numFloorLinks++;
                floorLink.name = "FloorLink" + numFloorLinks;                
                return true;
            }
        }
        return false;
    }

    // returns whether the endLink was set successfully. if not, the link will be destroyed
    // assumes more than one floor has been created
    private bool SetEnd(GameObject perimeterLink)
    {
        // gets the closest link game object (excluding floor links)
        GameObject closestLink = ClosestLink(perimeterLink);         

        float distToLink = Vector3.Distance(perimeterLink.transform.position, closestLink.transform.position);
        bool uniqueIfDesired = !completedLinks.Contains(perimeterLink.transform.position) && !completedLinks.Contains(closestLink.transform.position);
        if (!useUniqueLinks)
            uniqueIfDesired = true;

        if (distToLink < maxJumpDist && uniqueIfDesired)
        {
            perimeterLink.GetComponent<OffMeshLink>().endTransform = closestLink.transform;
            completedLinks.Add(perimeterLink.transform.position);
            completedLinks.Add(closestLink.transform.position);
            return true;
        }        
        return false;
    }

    // returns the closest link to the given link
    GameObject ClosestLink(GameObject newLink)
    {
        OffMeshLink[] links = FindObjectsOfType<OffMeshLink>();
        List<OffMeshLink> possibleLinks = new List<OffMeshLink>();
        foreach (OffMeshLink link in links)
        {
            // don't connect offmeshlinks on the same floor!
            bool diffFloor = !(perimiterLinks.Contains(link.gameObject));                        
            bool isFloorLink = Floor.floorLinks.Contains(link.startTransform.position);
            
            if (diffFloor && !isFloorLink)                            
                possibleLinks.Add(link);            
        }

        if (possibleLinks.Count == 0)
            return null;

        float dist;
        float minDist = Mathf.Infinity;
        int closestIndex = 0;
        for (int i = 0; i < possibleLinks.Count; i++)
        {
            dist = Vector3.Distance(newLink.transform.position, possibleLinks[i].transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestIndex = i;
            }
        }

        return possibleLinks[closestIndex].gameObject;
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

    // Update is called once per frame
    void Update()
    {

    }
}