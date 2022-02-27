using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGridAnimator3D : MonoBehaviour
{
    //public bool isMoving = false;

    // set in inspector
    public AnimationCurve movementCurve;
    public float movementDuration = 1;

    public class OnTileMoveArgs3D : System.EventArgs
    {
        public STile3D stile;
        public Vector3Int prevPos;
        public SMove3D smove; // the SMove3D this Move() was a part of
    }
    public static event System.EventHandler<OnTileMoveArgs3D> OnSTileMove;
    


    public void Move(SMove3D move)
    {
        STile3D[,,] grid = SGrid3D.current.GetGrid();

        Dictionary<Vector3Int, List<int>> borders = move.GenerateBorders();
        StartCoroutine(DisableBordersAndColliders(grid, SGrid3D.current.GetBGGrid(), move.positions, borders));

        foreach (Movement m in move.moves)
        {
            if (grid[m.startLoc.x, m.startLoc.y, m.startLoc.z].isTileActive)
            {
                StartCoroutine(StartMovingAnimation(grid[m.startLoc.x, m.startLoc.y, m.startLoc.z], m, move));
            }
            else
            {
                grid[m.startLoc.x, m.startLoc.y, m.startLoc.z].SetGridPosition(m.endLoc.x, m.endLoc.y, m.endLoc.z);
            }
        }

    }

    // move is only here so we can pass it into the event
    private IEnumerator StartMovingAnimation(STile3D stile, Movement moveCoords, SMove3D move)
    {
        float t = 0;
        //isMoving = true;
        stile.SetMovingDirection(GetMovingDirection(moveCoords.startLoc, moveCoords.endLoc));
        
        stile.SetBorderColliders(true);

        StartCoroutine(StartCameraShakeEffect());

        while (t < movementDuration)
        {
            float s = movementCurve.Evaluate(t / movementDuration);
            Vector3 pos = Vector3.Lerp(moveCoords.startLoc, moveCoords.endLoc, s);
            //Vector3 pos = (1 - s) * orig + s * target;
            
            stile.SetMovingPosition(pos);

            yield return null;
            t += Time.deltaTime;
        }

        //isMoving = false;
        
        stile.SetBorderColliders(false);
        //for (int i = 0; i < collidersInactive.Count; i++)
        //{
        //    if (collidersInactive[i].SetSliderCollider(true))
        //    {
        //        collidersInactive.RemoveAt(i);
        //        i--;
        //    }
        //}

        stile.SetMovingDirection(Vector3.zero);
        stile.SetGridPosition(moveCoords.endLoc);

        InvokeOnSTileMove(stile, moveCoords.startLoc, move);
    }

    private IEnumerator DisableBordersAndColliders(STile3D[,,] grid, SGridBackground3D[,,] bgGrid, HashSet<Vector3Int> positions, Dictionary<Vector3Int, List<int>> borders)
    {
        foreach (Vector3Int p in borders.Keys)
        {
            if (0 <= p.x && p.x < bgGrid.GetLength(0) && 0 <= p.y && p.y < bgGrid.GetLength(1) && 0 <= p.z && p.z < bgGrid.GetLength(2))
            {
                foreach (int i in borders[p])
                {
                    bgGrid[p.x, p.y, p.z].SetBorderCollider(i, true);
                }
            }
        }

        List<STile3D> disabledColliders = new List<STile3D>();

        // if the player is on a slider, disable hitboxes temporarily
        foreach (Vector3Int p in positions)
        {
            if (Player.GetStileUnderneath() != grid[p.x, p.y, p.z].islandId)
            {
                //Debug.Log("disabling" +  p.x + " " + p.y);
                grid[p.x, p.y, p.z].SetSliderCollider(false);
                disabledColliders.Add(grid[p.x, p.y, p.z]);
            }
        }

        yield return new WaitForSeconds(movementDuration);

        foreach (Vector3Int p in borders.Keys)
        {
            if (0 <= p.x && p.x < bgGrid.GetLength(0) && 0 <= p.y && p.y < bgGrid.GetLength(1) && 0 <= p.z && p.z < bgGrid.GetLength(2))
            {
                foreach (int i in borders[p])
                {
                    bgGrid[p.x, p.y, p.z].SetBorderCollider(i, false);
                }
            }
        }

        foreach (STile3D t in disabledColliders)
        {
            t.SetSliderCollider(true);
        }
    }

    private Vector2 GetMovingDirection(Vector3 start, Vector3 end) // include magnitude?
    {
        Vector3 returnVector = Vector3.Normalize(start - end);
        if(returnVector == Vector3.zero)
            Debug.LogError("Moving Tile to the same spot!");
        return returnVector;
    }

    private IEnumerator StartCameraShakeEffect()
    {
        CameraShake.ShakeConstant(movementDuration + 0.1f, 0.15f);
        AudioManager.Play("Slide Rumble");

        yield return new WaitForSeconds(movementDuration);

        CameraShake.Shake(0.5f, 1f);
        AudioManager.Play("Slide Explosion");

    }


    private void InvokeOnSTileMove(STile3D s, Vector3Int prevPos, SMove3D m)
    {
        OnSTileMove?.Invoke(this, new OnTileMoveArgs3D { 
            stile = s, prevPos = prevPos, smove = m 
        });
    }
}
