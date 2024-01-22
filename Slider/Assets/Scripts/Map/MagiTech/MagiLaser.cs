using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiLaser : MonoBehaviour, ISavable
{
    [SerializeField] private Vector2 initDir = Vector2.left;

    public bool isPowered;
    public bool isEnabled;
    public string SaveString;
    public List<LineRenderer> lineRenderers;
    private RaycastHit2D hit;
    private Laserable laserable;
    // private Vector2 curDir, curDir2;
    // private Vector2 curPos, curPos2;
    private List<Laserable> lastFrameLaserables = new();
    private List<Laserable> thisFrameLaserables = new();

    [SerializeField] private MagiLaserAnimation magiLaserAnimation;
    [SerializeField] private MagiLaserFlashManager magiLaserFlashManager;
    [SerializeField] private Transform emitPos;
    [SerializeField] private Laserable presentPortalLaserable;
    [SerializeField] private Laserable pastPortalLaserable;

    private const int MAX_LASER_BOUNCES = 32;
    private const float PORTAL_LASER_OFFSET = 1.5625f;

    public ArtifactTBPluginLaser laserUI;
    public UILaserManager uILaserManager;

    private void Start()
    {
        SetEnabled(isEnabled);
    }

    public void EnableLaser()
    {
        SetEnabled(true);
    }

    public void DisableLaser()
    {
        SetEnabled(false);
    }

    private void LateUpdate()
    {
        ClearLasers();
        MakeFirstLaser();
        UpdateLaserables();
    }

    public void ClearLasers()
    {
        //canShoot = false;
        foreach (LineRenderer lr in lineRenderers)
        {
            lr.positionCount = 0;
        }

        magiLaserFlashManager.ResetPool();
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
        Vector3 initPos = pastPortalLaserable.transform.position + offset + (Vector3)(initDir * PORTAL_LASER_OFFSET);
        DrawLaser(initDir, initPos, lineRenderers[1]);
    }

    private void MakeNewPresentLaser(Vector3 hitPosition, Vector2 initDir)
    {
        Vector3 offset = hitPosition - pastPortalLaserable.transform.position;
        Vector3 initPos = presentPortalLaserable.transform.position + offset + (Vector3)(initDir * PORTAL_LASER_OFFSET);
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
                Debug.LogWarning(hit.collider.gameObject.name + " does not have a Laserable component! Doing nothing");
                return;
            }

            thisFrameLaserables.Add(laserable);

            if (laserable.IsInteractionType("Passthrough"))
            {
                pos = hit.point + dir;
                continue;
            }

            lr.positionCount++;
            lr.SetPosition(lr.positionCount - 1, hit.point);

            if (laserable.IsInteractionType("Reflect")) 
            {
                dir = laserable.flipDirection ? MirrorTwoReflect(dir) : MirrorOneReflect(dir);
                pos = hit.point + dir;

                float angle = laserable.flipDirection ? 
                    ((dir.x > 0 || dir.y > 0) ? 45  : 225) :
                    ((dir.x < 0 || dir.y > 0) ? 135 : 315);
                magiLaserFlashManager.PutFlash(laserable.transform, hit.point, angle);

                continue;
            } 
            else if (laserable.IsInteractionType("Portal")) 
            {
                if (laserable == presentPortalLaserable && lr == lineRenderers[0])
                {
                    MakePastLaser(hit.point, dir);
                }
                else if (laserable == pastPortalLaserable)
                {
                    MakeNewPresentLaser(hit.point, dir);
                }
            }
            else if (laserable.IsInteractionType("Absorb")) 
            {
                float angle = Mathf.Atan2(-dir.y, -dir.x) * Mathf.Rad2Deg;
                magiLaserFlashManager.PutFlash(laserable.transform, hit.point, angle);
            }
            break;
        }
    }

    private void UpdateLaserables()
    {
        foreach(Laserable l in lastFrameLaserables)
        {
            if(!thisFrameLaserables.Contains(l))
                l.UnLaser();
        }
        foreach(Laserable l in thisFrameLaserables)
        {
            if(!lastFrameLaserables.Contains(l))
                l.Laser();
        }

        lastFrameLaserables.Clear();
        lastFrameLaserables.AddRange(thisFrameLaserables);
        thisFrameLaserables.Clear();
    }

    private Vector2 MirrorOneReflect(Vector2 dir) {
        return new Vector2(dir.y, dir.x);
    }

    private Vector2 MirrorTwoReflect(Vector2 dir){
        return new Vector2(-dir.y, -dir.x);
    }

    public void SetPowered(bool value)
    {
        if(value == isPowered) return;
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

        if(laserUI != null)
        {
            laserUI.Init();            
            uILaserManager.AddSource(laserUI.laserUIData);
        }

    }

    public void CheckIsPowered(Condition c) => c.SetSpec(isPowered);
    public void CheckIsEnabled(Condition c) => c.SetSpec(isEnabled);

    public void Save()
    {
        if(SaveString != null && SaveString != "")
            SaveSystem.Current.SetBool(SaveString, isEnabled);
    }

    public void Load(SaveProfile profile)
    {
        if(SaveString == null || SaveString == "") return;

        isEnabled = profile.GetBool(SaveString);
        if(isEnabled)
        {
            isPowered = true;
            magiLaserAnimation.PowerFromLoad();
        }
    }
}

