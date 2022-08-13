using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiLaser : MonoBehaviour
{
    public Vector2 initDir;

    private RaycastHit2D hit;
    private LineRenderer lineRenderer, lineRenderer2, lineRenderer3;
    private Vector2 curDir, curDir2, curDir3;
    private Vector2 curPos, curPos2, curPos3;
    private void Awake() {
        lineRenderer = transform.GetChild(0).GetComponent<LineRenderer>();
        lineRenderer2 = transform.GetChild(1).GetComponent<LineRenderer>();
        lineRenderer3 = transform.GetChild(2).GetComponent<LineRenderer>();
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
            hit = Physics2D.Raycast(curPos, curDir, 40.0f, 4096);
            if(hit){
                lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(lineRenderer.positionCount-1, hit.collider.transform.position);
                if(hit.collider.tag == "Mirror1") {
                    curDir = MirrorOneReflect(curDir);
                    curPos = hit.collider.transform.position + (Vector3)(curDir * 1.1f);
                    lineRenderer2.positionCount = 1;
                } else if(hit.collider.tag == "Mirror2") {
                    curDir = MirrorTwoReflect(curDir);
                    curPos = hit.collider.transform.position + (Vector3)(curDir * 1.1f);
                    lineRenderer2.positionCount = 1;
                } else if(hit.collider.tag == "Portal1") {
                    MakeLaser2(curDir);
                    incomplete = false;
                }
            } else {
                lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(lineRenderer.positionCount-1, curPos + 34.0f*curDir);
                lineRenderer2.positionCount = 1;
                incomplete = false;
            }
        }
    }
    private void MakeLaser2(Vector2 initDir2)
    {
        lineRenderer2.positionCount = 1;
        Vector3 initPos2 = GameObject.FindWithTag("Portal2").transform.position + (Vector3)(initDir2*0.8f);
        lineRenderer2.SetPosition(0,initPos2);
        curDir2 = initDir2;
        curPos2 = initPos2;
        bool incomplete = true;
        
        while(incomplete) {
            hit = Physics2D.Raycast(curPos2, curDir2, 40.0f, 4096);
            if(hit){
                lineRenderer2.positionCount += 1;
                lineRenderer2.SetPosition(lineRenderer2.positionCount-1, hit.collider.transform.position);
                if(hit.collider.tag == "Mirror1") {
                    curDir2 = MirrorOneReflect(curDir2);
                    curPos2 = hit.collider.transform.position + (Vector3)(curDir2 * 1.1f);
                } else if(hit.collider.tag == "Mirror2") {
                    curDir2 = MirrorTwoReflect(curDir2);
                    curPos2 = hit.collider.transform.position + (Vector3)(curDir2 * 1.1f);
                } else if(hit.collider.tag == "Portal2") {
                    incomplete = false;
                }
            } else {
                lineRenderer2.positionCount += 1;
                lineRenderer2.SetPosition(lineRenderer2.positionCount-1, curPos2 + 34.0f*curDir2);
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

