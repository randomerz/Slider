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
        public SMove smove; // the SMove this Move() was a part of
    }
    public static event System.EventHandler<OnTileMoveArgs> OnSTileMoveStart;
    public static event System.EventHandler<OnTileMoveArgs> OnSTileMoveEnd;

    public virtual void Move(SMove move, STile[,] grid = null)
    {
        if (grid == null)
        {
            grid = SGrid.current.GetGrid();
        }
        //STile[,] grid = SGrid.current.GetGrid();

        Dictionary<Vector2Int, List<int>> borders = move.GenerateBorders();
        StartCoroutine(DisableBordersAndColliders(grid, SGrid.current.GetBGGrid(), move.positions, borders));

        foreach (Movement m in move.moves)
        {
            if (grid[m.startLoc.x, m.startLoc.y].isTileActive)
            {
                StartCoroutine(StartMovingAnimation(grid[m.startLoc.x, m.startLoc.y], m, move));
            }
            else
            {
                grid[m.startLoc.x, m.startLoc.y].SetGridPosition(m.endLoc.x, m.endLoc.y);
            }
        }

        // DC: bug involving anchoring a tile in a rotate, lets you walk into void
        if (move is SMoveRotate) 
        {
            CheckAnchorInRotate(move as SMoveRotate, grid);
        }
    }

    // move is only here so we can pass it into the event
    protected IEnumerator StartMovingAnimation(STile stile, Movement moveCoords, SMove move)
    {
        float t = 0;
        //isMoving = true;
        bool isPlayerOnStile = (Player.GetStileUnderneath() != null &&
                                Player.GetStileUnderneath().islandId == stile.islandId);

        stile.SetMovingDirection(GetMovingDirection(moveCoords.startLoc, moveCoords.endLoc));
        
        if (isPlayerOnStile)
        {
            stile.SetBorderColliders(true);
        }

        OnSTileMoveStart?.Invoke(this, new OnTileMoveArgs
        {
            stile = stile,
            prevPos = moveCoords.startLoc,
            smove = move
        });

        StartCoroutine(StartCameraShakeEffect());

        while (t < movementDuration)
        {
            t += Time.deltaTime;    //L: This needs to be before evaluate, or else t won't reach 1 before loop exits.
            //Idk if Evaluate actually clamps or not
            float s = movementCurve.Evaluate(Mathf.Min(t / movementDuration, 1));
            Vector2 pos = Vector2.Lerp(moveCoords.startLoc, moveCoords.endLoc, s);
            //Vector3 pos = (1 - s) * orig + s * target;
            
            stile.SetMovingPosition(pos);

            yield return null;
        }

        //isMoving = false;
        
        if (isPlayerOnStile)
        {
            stile.SetBorderColliders(false);
        }
        //for (int i = 0; i < collidersInactive.Count; i++)
        //{
        //    if (collidersInactive[i].SetSliderCollider(true))
        //    {
        //        collidersInactive.RemoveAt(i);
        //        i--;
        //    }
        //}

        stile.SetGridPosition(moveCoords.endLoc);
        stile.SetMovingDirection(Vector2.zero);

        OnSTileMoveEnd?.Invoke(this, new OnTileMoveArgs
        {
            stile = stile,
            prevPos = moveCoords.startLoc,
            smove = move
        });
    }

    protected void InvokeOnStileMoveStart(STile stile, Movement moveCoords, SMove move) {
        OnSTileMoveStart?.Invoke(this, new OnTileMoveArgs
        {
            stile = stile,
            prevPos = moveCoords.startLoc,
            smove = move
        });
    }

    protected void InvokeOnStileMoveEnd(STile stile, Movement moveCoords, SMove move) {
        OnSTileMoveEnd?.Invoke(this, new OnTileMoveArgs
        {
            stile = stile,
            prevPos = moveCoords.startLoc,
            smove = move
        });
    }


    protected IEnumerator DisableBordersAndColliders(STile[,] grid, SGridBackground[,] bgGrid, HashSet<Vector2Int> positions, Dictionary<Vector2Int, List<int>> borders)
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
            // Debug.Log(Player.GetStileUnderneath());
            if (Player.GetStileUnderneath() != null && Player.GetStileUnderneath().islandId != grid[p.x, p.y].islandId)
            {
                // Debug.Log("disabling" +  p.x + " " + p.y);
                grid[p.x, p.y].SetSliderCollider(false);
                disabledColliders.Add(grid[p.x, p.y]);
            }
        }

        yield return new WaitForSeconds(movementDuration); // ideally this should be called with an event, not after time

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

    protected IEnumerator EnableTileBorderColliders(STile stile)
    {
        stile.SetBorderColliders(true);

        yield return new WaitForSeconds(movementDuration);

        stile.SetBorderColliders(false);
    }

    protected virtual Vector2 GetMovingDirection(Vector2 start, Vector2 end) // include magnitude?
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

    protected IEnumerator StartCameraShakeEffect()
    {
        CameraShake.ShakeConstant(movementDuration + 0.1f, 0.15f);
        AudioManager.Play("Slide Rumble");

        yield return new WaitForSeconds(movementDuration);

        CameraShake.Shake(0.5f, 1.0f); // todo: base this on movementDuration so that less camera shake if duration is lower
        AudioManager.Play("Slide Explosion");

    }



    

    private void CheckAnchorInRotate(SMoveRotate move, STile[,] grid)
    {
        // if player is on a stile that is anchored
        STile playerStile = Player.GetStileUnderneath();
        if (playerStile != null && playerStile.hasAnchor)
        {
            // Debug.Log("Player is on: " + playerStile.islandId);
            foreach (Vector2Int p in move.anchoredPositions)
            {
                // and that tile is involved in the rotation
                if (grid[p.x, p.y].isTileActive && grid[p.x, p.y].islandId == playerStile.islandId)
                {
                    // enable colliders temporarily
                    StartCoroutine(EnableTileBorderColliders(playerStile));
                    return;
                }
            }
        }
    }
}
