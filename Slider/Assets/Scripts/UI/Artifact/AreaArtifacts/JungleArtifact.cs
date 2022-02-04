using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JungleArtifact : UIArtifact
{
    protected override bool CheckAndSwap(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty, bool queuedMove)
    {
        STile[,] currGrid = SGrid.current.GetGrid();
        int x = buttonCurrent.x;
        int y = buttonCurrent.y;
        SMove linkedSwap = SMoveLinkedSwap.CreateInstance(x, y, buttonEmpty.x, buttonEmpty.y);
        if (linkedSwap == null)
        {
            return base.CheckAndSwap(buttonCurrent, buttonEmpty, queuedMove);
        } else
        {
            //L: Below is to handle the case for if you have linked tiles.
            int dx = buttonEmpty.x - x;
            int dy = buttonEmpty.y - y;

            Vector2Int linkCoords = SGrid.current.GetLinkTileCoords(SGrid.current.GetGrid(), x, y);
            int linkx = linkCoords.x;
            int linky = linkCoords.y;
            
            Vector4Int movecoords = new Vector4Int(linkx, linky, linkx + dx, linky + dy);
            if (SGrid.current.CanMove(linkedSwap) && (OpenPath(movecoords, SGrid.current.GetGrid()) || currGrid[linkx + dx, linky + dy] == currGrid[x, y]))
            {
                if (queuedMove)
                {
                    QueueCheckAndAdd(linkedSwap);
                }
                else
                {
                    SGrid.current.Move(linkedSwap);
                    StartCoroutine(WaitForMoveThenEmptyQueue(buttonCurrent));
                }

                //L: Swap the current button and the link button
                SwapButtons(buttonCurrent, buttonEmpty);
                SwapButtons(GetButton(linkx, linky), GetButton(linkx + dx, linky + dy));

                return true;
            }
            else
            {
                // Debug.Log("illegal");
                AudioManager.Play("Artifact Error");
                return false;
            }
        }
    }

    //Checks if the move can happen on the grid.
    //L: This should maybe be checked with GetMoveOptions?
    private bool OpenPath(Vector4Int move, STile[,] grid)
    {
        List<Vector2Int> checkedCoords = new List<Vector2Int>();
        int dx = move.z - move.x;
        int dy = move.w - move.y;
        // Debug.Log(move.x+" "+move.y+" "+move.z+" "+move.w);
        int toCheck = Math.Max(Math.Abs(dx), Math.Abs(dy));
        if (dx == 0)
        {
            int dir = dy / Math.Abs(dy);
            for (int i = 1; i <= toCheck; i++)
            {
                if (grid[move.x, move.y + i * dir].isTileActive)
                {
                    return false;
                }
            }
        }
        else if (dy == 0)
        {
            int dir = dx / Math.Abs(dx);
            for (int i = 1; i <= toCheck; i++)
            {
                if (grid[move.x + i * dir, move.y].isTileActive)
                {
                    return false;
                }
            }
        }
        return true;
    }
}

