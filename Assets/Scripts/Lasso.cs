/*
@Authors - Patrick
@Description - Lasso functionality. New version with grapple hook functionality rather than rope funcitonality.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Lasso : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Vector3 grapplePoint;
    //private SpringJoint joint;
    
    private float lassoYOffset = 0; //0.25f
    private float overshootYAxis = 0;  //5f

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
        if (Player.player.usingLasso()){
            DrawRope();
        }

        //jank that prevents stupid visual bug where rope is suspended in the air after going from hanging to ground
        if (Player.player.currentMovementState == Player.movementState.HANGING){
            RaycastHit hit;

            if (Physics.Raycast(camera.transform.position, camera.transform.up * -1, out hit, 0.6f)) 
            {
                Vector3 downwardPoint = hit.point;

                if (hit.transform.gameObject.tag == "FLOOR")
                {
                    EndLasso();
                }
            }
        }
    }

    public bool StartLasso()
    {
        RaycastHit hit;

        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, Player.player.maxLassoRange)) {
            grapplePoint = hit.point;

            if (!isValidLassoObj(hit.transform.gameObject))
            {
                return false;
            }

            Player.player.currentHook = hit.transform.gameObject;

            LassoPhysics();
            lineRenderer.positionCount = 2;

            return true;
        }
      
        return false;
    }

    public void ContinueLasso(){
        LassoPhysics();   
    }

    private void LassoPhysics(){
        float grapplePointRelativeYPosition = grapplePoint.y - Player.player.playerFeetPosition();
        float highestPointOnArcTrajectory = grapplePointRelativeYPosition - lassoYOffset;
        
        if(grapplePointRelativeYPosition < 0)
        {
            highestPointOnArcTrajectory = overshootYAxis;
        }

        Player.player.lassoLaunch(grapplePoint, highestPointOnArcTrajectory);
    }

    public void EndLasso()
    {
        lineRenderer.positionCount = 0;
    }

    public void DrawRope()
    {
        if (lineRenderer.positionCount < 2)
        {
            return;
        }

        lineRenderer.SetPosition(0, lassoTip.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }

    public bool isValidLassoObj(GameObject obj){
        return (obj.tag == "HOOK" || obj.tag == "BARREL" || obj.tag == "TORNADO");
    }
}

