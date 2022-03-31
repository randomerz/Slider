using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JungleArtifact : UIArtifact
{
    // private static STile prevLinkTile = null;

    protected override bool CheckAndSwap(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        if (buttonCurrent.linkButton == null)
        {
            //L: Just a normal move
            return base.CheckAndSwap(buttonCurrent, buttonEmpty);
        } 
        else
        {
            //L: Below is to handle the case for if you have linked tiles.
            STile[,] currGrid = SGrid.current.GetGrid();
            int x = buttonCurrent.x;
            int y = buttonCurrent.y;

            //L: Make sure that all checks/swaps are with respect to the UI and NOT the grid (bc the grid can be behind due to queuing)
            //Debug.Log("Linked Move!");
            int linkx = buttonCurrent.linkButton.x;
            int linky = buttonCurrent.linkButton.y;
            int dx = buttonEmpty.x - x;
            int dy = buttonEmpty.y - y;

            SMove linkedSwap = new SMoveLinkedSwap(x, y, buttonEmpty.x, buttonEmpty.y, linkx, linky,
                                                    buttonCurrent.islandId, buttonCurrent.linkButton.islandId);

            Movement movecoords = new Movement(linkx, linky, linkx + dx, linky + dy, buttonCurrent.islandId);
            if (SGrid.current.CanMove(linkedSwap) && (OpenPath(movecoords) || GetButton(linkx + dx, linky + dy) == buttonCurrent) && moveQueue.Count < maxMoveQueueSize)
            {
                QueueCheckAndAdd(linkedSwap);

                //L: Swap the current button and the link button
                SwapButtons(buttonCurrent, buttonEmpty);
                SwapButtons(buttonCurrent.linkButton, GetButton(linkx + dx, linky + dy));

                QueueCheckAfterMove(this, null);
                // if (moveQueue.Count == 1)
                // {
                //     SGrid.current.Move(moveQueue.Peek());
                // }

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
    private bool OpenPath(Movement move)
    {
        List<Vector2Int> checkedCoords = new List<Vector2Int>();
        int dx = move.endLoc.x - move.startLoc.x;
        int dy = move.endLoc.y - move.startLoc.y;
        // Debug.Log(move.x+" "+move.y+" "+move.z+" "+move.w);
        int toCheck = Math.Max(Math.Abs(dx), Math.Abs(dy));
        if (dx == 0)
        {
            int dir = dy / Math.Abs(dy);
            for (int i = 1; i <= toCheck; i++)
            {
                if (GetButton(move.startLoc.x, move.startLoc.y + i * dir).isTileActive)
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
                if (GetButton(move.startLoc.x + i * dir, move.startLoc.y).isTileActive)
                {
                    return false;
                }
            }
        }
        return true;
    }
}

