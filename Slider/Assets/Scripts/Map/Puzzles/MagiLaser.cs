using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiLaser : MonoBehaviour
{
    public Vector2 initDir;

    public bool isEnabled;
    public LineRenderer lineRenderer, lineRenderer2, lineRenderer3;
    private RaycastHit2D hit;
    private Laserable laserable;
    private Vector2 curDir, curDir2, curDir3;
    private Vector2 curPos, curPos2, curPos3;
    private LinkedList<Vector3> rendererPositions, pastRendererPositions; 
    void Update()
    {
        MakeLaser();
    }

    private void OnSTileMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (!isEnabled || !UIArtifact.GetInstance().MoveQueueEmpty()) return;
        MakeLaser();
    }

    private void OnSTileMoveStart(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        ClearLasers();
    }

    public void ClearLasers()
    {
        lineRenderer.positionCount = 0;
        lineRenderer2.positionCount = 0;
    }

    private void MakeLaser()
    {
        Vector3 initPos = transform.position + new Vector3(-3.0f,1.0f,0.0f);
        curDir = initDir;
        curPos = initPos;
        lineRenderer2.positionCount = 0;
        DrawLaser(curDir, curPos, lineRenderer);
    }
    private void MakePastLaser(Transform transform, Vector2 initDir)
    {
        Vector3 initPos = transform.position + (Vector3)(initDir*0.8f) + (Vector3.right*100); //Distance to past portal is <100,0,0>
        curDir2 = initDir;
        curPos2 = initPos;
        DrawLaser(curDir2, curPos2, lineRenderer2);
    }
    private void MakeLaser3(Vector2 initDir3)
    {
        Vector3 initPos3 = GameObject.FindWithTag("Portal1").transform.position + (Vector3)(initDir3*0.8f); 
        curDir3 = initDir3;
        curPos3 = initPos3;
        DrawLaser(curDir3, curPos3, lineRenderer3);
    }
    
    /// <summary>
    /// Given a LineRenderer, Raycasts to check for laser interactions and updates list of positions
    /// to set LineRenderer positions to
    /// </summary>
    /// <param name="dir"> direction to raycast</param>
    /// <param name="pos"> starting position to raycast from</param>
    /// <param name="lr"> LineRenderer to set positions for</param>
    private void DrawLaser(Vector2 dir, Vector2 pos, LineRenderer lr) 
    {
        //Set origin point
        lr.positionCount = 1;
        lr.SetPosition(lr.positionCount - 1, pos);
        // bool tp1, tp2 = false;

        while(true) {
            hit = Physics2D.Raycast(pos, dir, 40.0f, 4096); //4096 is layermask RayCast
            if (!hit)
            {
                lr.positionCount++;
                lr.SetPosition(lr.positionCount - 1, pos + 34.0f * dir);
                break;
            }

            laserable = hit.collider.GetComponent<Laserable>();

            if (!laserable)
            {
                Debug.LogWarning(hit + " does not have a Laserable component! Doing nothing");
                return;
            }

            lr.positionCount++;
            lr.SetPosition(lr.positionCount - 1, hit.transform.position);

            if (laserable.laserInteractionType == Laserable.LaserInteractionType.Reflect) {
                dir = laserable.flipDirection ? MirrorTwoReflect(dir) : MirrorOneReflect(dir);
                pos = hit.transform.position + (Vector3)(dir * 1.1f);
            } else if(laserable.laserInteractionType == Laserable.LaserInteractionType.Portal) {
                MakePastLaser(hit.transform, dir);
                break;
            } else if (laserable.laserInteractionType == Laserable.LaserInteractionType.Absorb){
                laserable.OnLasered?.Invoke();
                break;
            }
        }
    }
    private Vector2 MirrorOneReflect(Vector2 dir) {
        return new Vector2(dir.y, dir.x);
    }
    private Vector2 MirrorTwoReflect(Vector2 dir){
        return new Vector2(-dir.y, -dir.x);
    }
}

