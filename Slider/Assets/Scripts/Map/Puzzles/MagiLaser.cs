using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiLaser : MonoBehaviour
{
    public Vector2 initDir;

    public bool isEnabled;
    public LineRenderer lineRenderer, lineRenderer2;
    private RaycastHit2D hit;
    private Laserable laserable;
    private Vector2 curDir, curDir2;
    private Vector2 curPos, curPos2;

    private void OnEnable()
    {
        //SGridAnimator.OnSTileMoveEnd += OnSTileMoveEnd;
        //UIArtifact.MoveMadeOnArtifact += OnMoveMadeOnArtifact;
    }

    private void OnDisable()
    {
        //SGridAnimator.OnSTileMoveEnd -= OnSTileMoveEnd;
        //UIArtifact.MoveMadeOnArtifact -= OnMoveMadeOnArtifact;
    }
    private void FixedUpdate()
    {
        MakeLaser();
    }
    /*
    private void OnSTileMoveEnd(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (!isEnabled && UIArtifact.GetInstance().MoveQueueEmpty()) return;
        canShoot = true;
    }
    
    private void OnMoveMadeOnArtifact(object sender, System.EventArgs e)
    {
        ClearLasers();
    }
    */
    public void ClearLasers()
    {
        //canShoot = false;
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
    
    /// <summary>
    /// Given a LineRenderer, Raycasts to check for laser interactions and updates list of positions
    /// to set LineRenderer positions to
    /// </summary>
    /// <param name="dir"> direction to raycast</param>
    /// <param name="pos"> starting position to raycast from</param>
    /// <param name="lr"> LineRenderer to set positions for</param>
    private void DrawLaser(Vector2 dir, Vector2 pos, LineRenderer lr) 
    {
        if (!isEnabled) return;
        //Set origin point
        lr.positionCount = 1;
        lr.SetPosition(lr.positionCount - 1, pos);
        // bool tp1, tp2 = false;
        for(int ct = 0; ct < 10; ct++) {
            hit = Physics2D.Raycast(pos, dir, 50.0f, 4096); //4096 is layermask RayCast
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

            laserable.isLasered = true;
            laserable.OnLasered?.Invoke();

            if (laserable.IsInteractionType("Passthrough"))
            {
                pos = hit.transform.position + (Vector3)(dir * 1.1f);
                continue;
            }

            lr.positionCount++;
            lr.SetPosition(lr.positionCount - 1, laserable.IsInteractionType("Absorb") ? hit.point : hit.transform.position);

            if (laserable.IsInteractionType("Reflect")) {
                dir = laserable.flipDirection ? MirrorTwoReflect(dir) : MirrorOneReflect(dir);
                pos = hit.transform.position + (Vector3)(dir * 1.1f);
                continue; //Only time we want to continue is if laser hits a mirror
            } 
            else if(laserable.IsInteractionType("Portal")) {
                MakePastLaser(hit.transform, dir); //Will make another laser
            }
            break;
        }
    }
    private Vector2 MirrorOneReflect(Vector2 dir) {
        return new Vector2(dir.y, dir.x);
    }
    private Vector2 MirrorTwoReflect(Vector2 dir){
        return new Vector2(-dir.y, -dir.x);
    }

    public void ToggleLaser()
    {
        isEnabled = !isEnabled;
    }
}

