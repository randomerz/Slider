using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainSGridAnimator : SGridAnimator
{
     //public bool isMoving = false;

    // set in inspector


    public override void Move(SMove move, STile[,] grid = null)
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
                if (Mathf.Abs(m.endLoc.y - m.startLoc.y) <= 25)
                StartCoroutine(StartMovingAnimation(grid[m.startLoc.x, m.startLoc.y], m, move));
                else
                StartCoroutine(StartLayerMovingAnimation(grid[m.startLoc.x, m.startLoc.y], m, move));
            }
            else
            {
                grid[m.startLoc.x, m.startLoc.y].SetGridPosition(m.endLoc.x, m.endLoc.y);
            }
        }
    }

    
    // move is only here so we can pass it into the event
    private IEnumerator StartLayerMovingAnimation(STile stile, Movement moveCoords, SMove move)
    {
        //isMoving = true;
        bool isPlayerOnStile = (Player.GetStileUnderneath() != null &&
                                Player.GetStileUnderneath().islandId == stile.islandId);

        stile.SetMovingDirection(GetMovingDirection(moveCoords.startLoc, moveCoords.endLoc));
        
        if (isPlayerOnStile)
        {
            stile.SetBorderColliders(true);
        }

        base.InvokeOnStileMoveStart(stile, moveCoords, move);

        StartCoroutine(StartCameraShakeEffect());

        yield return new WaitForSeconds(movementDuration);

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

        stile.SetMovingDirection(Vector2.zero);
        stile.SetGridPosition(moveCoords.endLoc);

        base.InvokeOnStileMoveEnd(stile, moveCoords, move);
    }

    //TODO: Colliders :meownotlikethis:
    /*
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

    private IEnumerator EnableTileBorderColliders(STile stile)
    {
        stile.SetBorderColliders(true);

        yield return new WaitForSeconds(movementDuration);

        stile.SetBorderColliders(false);
    }*/

    protected override Vector2 GetMovingDirection(Vector2 start, Vector2 end) // include magnitude?
    {
        return start - end;
    }
}
