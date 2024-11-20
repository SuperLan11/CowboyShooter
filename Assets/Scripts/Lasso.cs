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
    private float lassoMaxRange = 100f;
    
    //!Make sure you keep track of this cause it's in the editor;
    [SerializeField] private float lassoYOffset = 0.25f;
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

        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, lassoMaxRange)) {
            grapplePoint = hit.point;

            if (!isValidLassoObj(hit.transform.gameObject))
            {
                return false;
            }

            float grapplePointRelativeYPosition = grapplePoint.y - Player.player.playerFeetPosition();
            float highestPointOnArcTrajectory = grapplePointRelativeYPosition - lassoYOffset;

            
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

    public bool usingLasso()
    {
        return (Player.player.currentMovementState == Player.movementState.HANGING ||
            Player.player.currentMovementState == Player.movementState.SWINGING);
    }

    public bool isValidLassoObj(GameObject obj){
        return (obj.tag == "HOOK" || obj.tag == "BARREL");
    }
}

