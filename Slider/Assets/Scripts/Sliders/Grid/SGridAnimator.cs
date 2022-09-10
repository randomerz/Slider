using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGridAnimator : MonoBehaviour
{

    // set in inspector
    public AnimationCurve movementCurve;
    protected float movementDuration = 1f;

    public class OnTileMoveArgs : System.EventArgs
    {
        public STile stile;
        public Vector2Int prevPos;
        public SMove smove; // the SMove this Move() was a part of
        public float moveDuration; // base * smove.moveduration
    }
    public static event System.EventHandler<OnTileMoveArgs> OnSTileMoveStart;
    public static event System.EventHandler<OnTileMoveArgs> OnSTileMoveEndEarly;
    public static event System.EventHandler<OnTileMoveArgs> OnSTileMoveEnd;

    private float currMoveDuration = 1f;

    public void ChangeMovementDuration(float value)
    {
        movementDuration = value;
    }


    public float GetMovementDuration()
    {
        return movementDuration;
    }


    public virtual void Move(SMove move, STile[,] grid = null)
    {
        if (grid == null)
        {
            grid = SGrid.Current.GetGrid();
        }

        List<Coroutine> moveCoroutines = new List<Coroutine>();
        foreach (Movement m in move.moves)
        {
            if (grid[m.startLoc.x, m.startLoc.y].isTileActive)
                moveCoroutines.Add(DetermineMoveType(move, grid, m));
            else
                grid[m.startLoc.x, m.startLoc.y].SetGridPosition(m.endLoc.x, m.endLoc.y);
        }
        
        StartCoroutine(DisableBordersAndColliders(grid, SGrid.Current.GetBGGrid(), move, moveCoroutines));

    }

    //C: Added to avoid duplicated code in mountian section
    protected virtual Coroutine DetermineMoveType(SMove move, STile[,] grid, Movement m)
    {
        return StartCoroutine(StartMovingAnimation(grid[m.startLoc.x, m.startLoc.y], m, move));
    }

    // move is only here so we can pass it into the event
    // C: if animate is true, will animate to destination (this is the case 99% of the time)
    // if animate is false, will wait and then TP to destination (ex. mountain going up/down)
    protected IEnumerator StartMovingAnimation(STile stile, Movement moveCoords, SMove move, bool animate = true)
    {
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
            smove = move,
            moveDuration = currMoveDuration
        });



        EffectOnMoveStart(move is SMoveConveyor);

        float t = 0;
        currMoveDuration = movementDuration * move.duration;
        while (t < currMoveDuration)
        {
            t += Time.deltaTime;
            
            if(animate)
            {
                float s = movementCurve.Evaluate(Mathf.Min(t / currMoveDuration, 1));
                Vector2 pos = Vector2.Lerp(moveCoords.startLoc, moveCoords.endLoc, s);
                stile.SetMovingPosition(pos);
            }
            yield return null;
        }
        
        if (isPlayerOnStile)
        {
            stile.SetBorderColliders(false);
        }

        stile.SetGridPosition(moveCoords.endLoc);
        stile.SetMovingDirection(Vector2.zero);

        OnSTileMoveEndEarly?.Invoke(this, new OnTileMoveArgs
        {
            stile = stile,
            prevPos = moveCoords.startLoc,
            smove = move,
            moveDuration = currMoveDuration
        });

        OnSTileMoveEnd?.Invoke(this, new OnTileMoveArgs
        {
            stile = stile,
            prevPos = moveCoords.startLoc,
            smove = move,
            moveDuration = currMoveDuration
        });
        EffectOnMoveFinish();
    }

    // DC: this is a lot of parameters :)

    protected IEnumerator DisableBordersAndColliders(
            STile[,] grid, 
            SGridBackground[,] bgGrid,
            SMove move, 
            List<Coroutine> moveCoroutines
        )
    {
        Dictionary<Vector2Int, List<int>> borders = move.GenerateBorders();

        // disable borders colliders
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

        // DC: bug involving anchoring a tile in a rotate, lets you walk into void
        STile anchorRotationPlayerStile = null;
        if (move is SMoveRotate) 
        {
            anchorRotationPlayerStile = CheckAnchorInRotate(move as SMoveRotate, grid);
            anchorRotationPlayerStile?.SetBorderColliders(true);
        }

        List<STile> disabledColliders = new List<STile>();

        // if the player is on a slider, disable hitboxes temporarily
        foreach (Vector2Int p in move.positions)
        {
            // Debug.Log(Player.GetStileUnderneath());
            if (Player.GetStileUnderneath() != null && Player.GetStileUnderneath().islandId != grid[p.x, p.y].islandId)
            {
                // Debug.Log("disabling" +  p.x + " " + p.y);
                grid[p.x, p.y].SetSliderCollider(false);
                disabledColliders.Add(grid[p.x, p.y]);
            }
        }


        // Wait for moves to finish
        foreach (Coroutine coroutine in moveCoroutines)
        {
            yield return coroutine;
        }


        // enable border colliders
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

        // Anchor bug
        anchorRotationPlayerStile?.SetBorderColliders(false);

        foreach (STile t in disabledColliders)
        {
            t.SetSliderCollider(true);
        }
    }

    // returns the Stile which should have its border colliders enabled
    private STile CheckAnchorInRotate(SMoveRotate move, STile[,] grid)
    {
        // if player is on a stile that is anchored
        STile playerStile = Player.GetStileUnderneath();
        if (playerStile != null && playerStile.hasAnchor)
        {
            foreach (Vector2Int p in move.anchoredPositions)
            {
                // and that tile is involved in the rotation
                if (grid[p.x, p.y].isTileActive && grid[p.x, p.y].islandId == playerStile.islandId)
                {
                    // return that tile so its borders are enabled
                    return playerStile;
                }
            }
        }
        return null;
    }

    protected void EffectOnMoveStart(bool isConveyor)
    {
        CameraShake.ShakeConstant(currMoveDuration + 0.1f, 0.15f);
        AudioManager.PlayWithVolume(isConveyor ? "Conveyor" : "Slide Rumble", currMoveDuration);
    }

    protected void EffectOnMoveFinish()
    {
        //L: Bruh I can't
        //bool moveToConveyor = false;
        //List<SMove> activeMoves = UIArtifact.GetActiveMoves();
        //activeMoves.ForEach(move =>
        //{
        //    if (move is SMoveConveyor)
        //    {
        //        moveToConveyor = true;
        //    }
        //});

        CameraShake.Shake(currMoveDuration / 2, 1.0f);
        AudioManager.PlayWithVolume("Slide Explosion", currMoveDuration);
    }

    protected virtual Vector2 GetMovingDirection(Vector2 start, Vector2 end) 
    {
        Vector2 dif = start - end;
        return dif.magnitude > 0.1 ? dif : Vector2.zero; //C: in case of float jank
    }
}
