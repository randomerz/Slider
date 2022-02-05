using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JungleArtifact : UIArtifact
{
    private static STile prevLinkTile = null;

    protected override bool CheckAndSwap(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        STile[,] currGrid = SGrid.current.GetGrid();
        int x = buttonCurrent.x;
        int y = buttonCurrent.y;
        if (buttonCurrent.linkButton == null)
        {
            Debug.Log("Normal Move!");
            //L: Just a normal move
            return base.CheckAndSwap(buttonCurrent, buttonEmpty);
        } else
        {
            Debug.Log("Linked Move!");
            //L: Below is to handle the case for if you have linked tiles.
            int linkx = buttonCurrent.linkButton.x;
            int linky = buttonCurrent.linkButton.y;
            int dx = buttonEmpty.x - x;
            int dy = buttonEmpty.y - y;

            SMove linkedSwap = new SMoveLinkedSwap(x, y, buttonEmpty.x, buttonEmpty.y, linkx, linky);

            Vector4Int movecoords = new Vector4Int(linkx, linky, linkx + dx, linky + dy);
            if (SGrid.current.CanMove(linkedSwap) && (OpenPath(movecoords, SGrid.current.GetGrid()) || currGrid[linkx + dx, linky + dy] == currGrid[x, y]))
            {
                QueueCheckAndAdd(linkedSwap);

                //L: Swap the current button and the link button
                SwapButtons(buttonCurrent, buttonEmpty);
                SwapButtons(buttonCurrent.linkButton, GetButton(linkx + dx, linky + dy));

                if (moveQueue.Count == 1)
                {
                    SGrid.current.Move(moveQueue.Peek());
                }

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

    protected override void QueueCheckAfterMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        //L: This prevents the method from checking the queue twice if there are linked tiles (since it's called for every tile that invokes OnSTileMove)
        if (prevLinkTile == null || prevLinkTile != e.stile.linkTile)
        {
            base.QueueCheckAfterMove(sender, e);
            prevLinkTile = e.stile;
        } else
        {
            //L: Note: this only works if there's one link tile.
            prevLinkTile = null;
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

