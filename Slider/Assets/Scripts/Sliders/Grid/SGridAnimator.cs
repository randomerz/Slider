using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGridAnimator : MonoBehaviour
{
    //public bool isMoving = false;

    // set in inspector
    public AnimationCurve movementCurve;
    public float movementDuration = 1;

    public class OnTileMoveArgs : System.EventArgs
    {
        public STile stile;
        public Vector2Int prevPos;
    }
    public static event System.EventHandler<OnTileMoveArgs> OnSTileMove;
    


    public void Move(SMove move)
    {
        STile[,] grid = SGrid.current.GetGrid();

        Dictionary<Vector2Int, List<int>> borders = move.GenerateBorders();
        StartCoroutine(DisableBordersAndColliders(grid, SGrid.current.GetBGGrid(), move.positions, borders));

        foreach (Vector4Int m in move.moves)
        {
            if (grid[m.x, m.y].isTileActive)
            {
                StartCoroutine(StartMovingAnimation(grid[m.x, m.y], m));
            }
            else
            {
                grid[m.x, m.y].SetGridPosition(m.z, m.w);
            }
        }

    }

    private IEnumerator StartMovingAnimation(STile stile, Vector4Int moveCoords)
    {
        float t = 0;
        //isMoving = true;

        Vector2 start = new Vector2(moveCoords.x, moveCoords.y);
        Vector2 end = new Vector2(moveCoords.z, moveCoords.w);
        stile.SetMovingDirection(GetMovingDirection(start, end));
        
        stile.SetBorderColliders(true);

        StartCoroutine(StartCameraShakeEffect());

        while (t < movementDuration)
        {
            float s = movementCurve.Evaluate(t / movementDuration);
            Vector2 pos = Vector2.Lerp(start, end, s);
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

        stile.SetMovingDirection(Vector2.zero);
        stile.SetGridPosition(moveCoords.z, moveCoords.w);

        InvokeOnSTileMove(stile, new Vector2Int(moveCoords.x, moveCoords.y));
    }

    private IEnumerator DisableBordersAndColliders(STile[,] grid, SGridBackground[,] bgGrid, HashSet<Vector2Int> positions, Dictionary<Vector2Int, List<int>> borders)
    {
        foreach (Vector2Int p in borders.Keys)
        {
            if (0 <= p.x && p.x < bgGrid.GetLength(0) && 0 <= p.y && p.y < bgGrid.GetLength(1))
            {
                foreach (int i in borders[p])
                {
                    bgGrid[p.x, p.y].SetBorderCollider(i, true);
                }
            }
        }

        List<STile> disabledColliders = new List<STile>();

        // if the player is on a slider, disable hitboxes temporarily
        foreach (Vector2Int p in positions)
        {
            if (Player.GetStileUnderneath() != grid[p.x, p.y].islandId)
            {
                //Debug.Log("disabling" +  p.x + " " + p.y);
                grid[p.x, p.y].SetSliderCollider(false);
                disabledColliders.Add(grid[p.x, p.y]);
            }
        }

        yield return new WaitForSeconds(movementDuration);

        foreach (Vector2Int p in borders.Keys)
        {
            if (0 <= p.x && p.x < bgGrid.GetLength(0) && 0 <= p.y && p.y < bgGrid.GetLength(1))
            {
                foreach (int i in borders[p])
                {
                    bgGrid[p.x, p.y].SetBorderCollider(i, false);
                }
            }
        }

        foreach (STile t in disabledColliders)
        {
            t.SetSliderCollider(true);
        }
    }

    private Vector2 GetMovingDirection(Vector2 start, Vector2 end) // include magnitude?
    {
        Vector2 dif = start - end;
        if (dif.x > 0)
        {
            return Vector2.right;
        }
        else if (dif.x < 0)
        {
            return Vector2.left;
        }
        else if (dif.y > 0)
        {
            return Vector2.up;
        }
        else if (dif.y < 0)
        {
            return Vector2.down;
        }
        else
        {
            Debug.LogError("Moving Tile to the same spot!");
            return Vector2.zero;
        }
    }

    private IEnumerator StartCameraShakeEffect()
    {
        CameraShake.ShakeConstant(movementDuration + 0.1f, 0.15f);
        AudioManager.Play("Slide Rumble");

        yield return new WaitForSeconds(movementDuration);

        CameraShake.Shake(0.5f, 1f);
        AudioManager.Play("Slide Explosion");

    }


    private void InvokeOnSTileMove(STile stile, Vector2Int prevPos)
    {
        OnSTileMove?.Invoke(this, new OnTileMoveArgs { stile = stile, prevPos = prevPos });
    }
}
