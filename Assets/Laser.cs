using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    private float laserRange = 30f;

    void Update()
    {
        Transform parent = this.gameObject.GetComponentInParent<Transform>();
        Vector3 parentPos = parent.position;
        
        lineRenderer.SetPosition(0, parentPos);

        RaycastHit hit;
        if (Physics.Raycast(parentPos, parent.forward, out hit))
        {
            lineRenderer.SetPosition(1, parent.forward * laserRange);
        }
    }
}
