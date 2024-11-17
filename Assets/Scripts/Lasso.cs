/*
@Authors - Patrick
@Description - Lasso functionality. New version with grapple hook functionality rather than rope funcitonality.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Lasso : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Vector3 grapplePoint;
    //private SpringJoint joint;
    private float lassoMaxRange = 100f;
    //currently 0, but if we want the player to grapple OVER an obj we can increase the value
    float lassoYOffset = 0;
    float overshootYAxis = 5f;

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

    public bool StartLasso()
    {
        RaycastHit hit;

        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, lassoMaxRange)) {
            grapplePoint = hit.point;

            if (!isValidLassoObj(hit.transform.gameObject))
            {
                return false;
            }

            float grapplePointRelativeYPosition = grapplePoint.y - Player.player.playerFeetPosition();
            float highestPointOnArcTrajectory = grapplePointRelativeYPosition + lassoYOffset;

            
            if(grapplePointRelativeYPosition < 0)
            {
                highestPointOnArcTrajectory = overshootYAxis;
            }
            

            Player.player.lassoLaunch(grapplePoint, highestPointOnArcTrajectory);
            lineRenderer.positionCount = 2;

            return true;
        }
      
        return false;
    }

    public void EndLasso()
    {
        lineRenderer.positionCount = 0;
        //Destroy(joint);
    }

    public void DrawRope()
    {
        /*
        if (joint == null){
            return;
        }
        */

        lineRenderer.SetPosition(0, lassoTip.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }

    public bool usingLasso()
    {
        return (Player.player.currentMovementState == Player.movementState.HANGING ||
            Player.player.currentMovementState == Player.movementState.SWINGING);
    }

    public bool isValidLassoObj(GameObject obj){
        return (obj.tag == "HOOK" || obj.tag == "BARREL");
    }
}

