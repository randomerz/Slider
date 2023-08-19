using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class MagiLaser : MonoBehaviour
{
    public Vector2 initDir;
    [SerializeField] private Transform emitPos;

    public bool isPowered;
    public bool isEnabled;
    public List<LineRenderer> lineRenderers;
    private RaycastHit2D hit;
    private Laserable laserable;
    // private Vector2 curDir, curDir2;
    // private Vector2 curPos, curPos2;

    public MagiLaserAnimation magiLaserAnimation;
    public Laserable presentPortalLaserable;
    public Laserable pastPortalLaserable;

    private const int MAX_LASER_BOUNCES = 32;

    private void Update()
    {
        ClearLasers();
        MakeFirstLaser();
    }

    public void ClearLasers()
    {
        //canShoot = false;
        foreach (LineRenderer lr in lineRenderers)
        {
            lr.positionCount = 0;
        }
    }

    private void MakeFirstLaser()
    {
        Vector2 curDir = initDir;
        Vector2 curPos = emitPos.position;
        // lineRenderer2.positionCount = 0;
        DrawLaser(curDir, curPos, lineRenderers[0]);
    }

    private void MakePastLaser(Vector3 hitPosition, Vector2 initDir)
    {
        Vector3 offset = hitPosition - presentPortalLaserable.transform.position;
        Vector3 initPos = pastPortalLaserable.transform.position + offset + (Vector3)(initDir * 1.6f);
        DrawLaser(initDir, initPos, lineRenderers[1]);
    }

    private void MakeNewPresentLaser(Vector3 hitPosition, Vector2 initDir)
    {
        Vector3 offset = hitPosition - pastPortalLaserable.transform.position;
        Vector3 initPos = presentPortalLaserable.transform.position + offset + (Vector3)(initDir * 0.8f);
        DrawLaser(initDir, initPos, lineRenderers[2]);
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

        // Set origin point
        lr.positionCount = 1;
        lr.SetPosition(lr.positionCount - 1, pos);
        
        for (int ct = 0; ct < MAX_LASER_BOUNCES; ct++) {
            hit = Physics2D.Raycast(pos, dir, 50.0f, LayerMask.GetMask("LaserRaycast"));
            if (!hit)
            {
                lr.positionCount++;
                lr.SetPosition(lr.positionCount - 1, pos + 50.0f * dir);
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
                // pos = hit.transform.position + (Vector3)(dir * 1.1f);
                pos = hit.point + dir;
                continue;
            }

            lr.positionCount++;
            // lr.SetPosition(lr.positionCount - 1, laserable.IsInteractionType("Absorb") ? hit.point : hit.transform.position);
            lr.SetPosition(lr.positionCount - 1, hit.point);

            if (laserable.IsInteractionType("Reflect")) {
                dir = laserable.flipDirection ? MirrorTwoReflect(dir) : MirrorOneReflect(dir);
                // pos = hit.transform.position + (Vector3)(dir * 1.1f);
                pos = hit.point + dir;
                continue;
            } 
            else if (laserable.IsInteractionType("Portal")) {
                if (laserable == presentPortalLaserable && lr != lineRenderers[2])
                {
                    MakePastLaser(hit.point, dir);
                }
                else if (laserable == pastPortalLaserable)
                {
                    MakeNewPresentLaser(hit.point, dir);
                }
                else
                {
                    Debug.LogWarning("Laser hit an unknown portal.");
                }
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

    public void SetPowered(bool value)
    {
        isPowered = value;
        magiLaserAnimation.SetPowered(value);
    }

    public void SetEnabled(bool value)
    {
        isEnabled = value;
        if (!value)
        {
            ClearLasers();
        }
    }
}

