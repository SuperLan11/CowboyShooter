/*
@Authors - Patrick
@Description - Lasso functionality
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Lasso : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Vector3 grapplePoint;
    private SpringJoint joint;
    private float lassoMaxRange = 100f;

    private float ropePull = 10f/*4.5f*/, ropeReduceForce = 7f;

    [SerializeField] private GameObject hook, barrel;
    [SerializeField] private Transform lassoTip;
    [SerializeField] private Camera camera;

    void Awake(){
        lineRenderer = GetComponent<LineRenderer>();

         if (lineRenderer == null){
            throw new System.Exception("Object doesn't have lineRenderer");
        }
    }

    void LateUpdate(){
        if (usingLasso()){
            DrawRope();
        }
    }

    public void StartLasso()
    {
        RaycastHit hit;

        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, lassoMaxRange)) {
            grapplePoint = hit.point;
            
            joint = Player.player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float lassoDistance = Vector3.Distance(Player.player.transform.position, grapplePoint);
            joint.maxDistance = lassoDistance * 0.8f;
            joint.minDistance = lassoDistance * 0.25f;

            joint.spring = ropePull;
            joint.damper = ropeReduceForce;
            //!idk that much about rope physics, so we should probably just avoid changing this
            joint.massScale = 4.5f;

            lineRenderer.positionCount = 2;
        }
        /*
        Debug.Log("lasso called");
        Vector3 newScale = lasso.transform.localScale;
        newScale.y *= 2;
        lasso.transform.localScale = newScale;*/

    }

    public void EndLasso()
    {
        lineRenderer.positionCount = 0;
        Destroy(joint);
    }

    public void DrawRope()
    {
        if (joint == null){
            return;
        }

        lineRenderer.SetPosition(0, lassoTip.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }

    public bool usingLasso()
    {
        return (Player.player.currentMovementState == Player.movementState.HANGING ||
            Player.player.currentMovementState == Player.movementState.SWINGING);
    }
}

