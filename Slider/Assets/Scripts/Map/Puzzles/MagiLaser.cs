using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiLaser : MonoBehaviour
{
    public Vector2 initDir;
    private LineRenderer lineRenderer;
    private RaycastHit2D hit;
    private Vector2 curDir;
    private Vector2 curPos;
    private void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
    }
    void Update()
    {
        MakeLaser();
    }
    private void LateUpdate() 
    {
        MakeLaser();
    }
    private void MakeLaser()
    {
        lineRenderer.positionCount = 1;
        Vector3 initPos = transform.position + new Vector3(-3.0f,1.0f,0.0f);
        lineRenderer.SetPosition(0,initPos);
        curDir = initDir;
        curPos = initPos;
        bool incomplete = true;
        
        while(incomplete) {
            hit = Physics2D.Raycast(curPos, curDir, 34.0f, 4096);
            if(hit){
                lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(lineRenderer.positionCount-1, hit.collider.transform.position);
                if(hit.collider.tag == "Mirror1") {
                    curDir = MirrorOneReflect(curDir);
                    curPos = hit.collider.transform.position + (Vector3)(curDir * 1.1f);
                } else if(hit.collider.tag == "Mirror2") {
                    curDir = MirrorTwoReflect(curDir);
                    curPos = hit.collider.transform.position + (Vector3)(curDir * 1.1f);
                } else if(hit.collider.tag == "Portal") {
                    
                    incomplete = false;
                }
            } else {
                lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(lineRenderer.positionCount-1, curPos + 34.0f*curDir);
                incomplete = false;
            }
        }
    }
    private Vector2 MirrorOneReflect(Vector2 dir) {
        if(dir ==  Vector2.up){
            return Vector2.right;
        } else if(dir ==  Vector2.right){
            return Vector2.up;
        } else if(dir ==  Vector2.down){
            return Vector2.left;
        } else {
            return Vector2.down;
        }
    }
    private Vector2 MirrorTwoReflect(Vector2 dir){
        if(dir ==  Vector2.up){
            return Vector2.left;
        } else if(dir ==  Vector2.right){
            return Vector2.down;
        } else if(dir ==  Vector2.down){
            return Vector2.right;
        } else {
            return Vector2.up;
        }
    }
}

