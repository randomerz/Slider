using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JungleArtifact : UIArtifact
{
    // private static STile prevLinkTile = null;

    public override bool TryQueueMoveFromButtonPair(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        if (buttonCurrent.LinkButton == null)
        {
            //L: Just a normal move
            return base.TryQueueMoveFromButtonPair(buttonCurrent, buttonEmpty);
        }
        else
        {
            //L: Below is to handle the case for if you have linked tiles.
            int x = buttonCurrent.x;
            int y = buttonCurrent.y;

            //L: Make sure that all checks/swaps are with respect to the UI and NOT the grid (bc the grid can be behind due to queuing)
            int linkx = buttonCurrent.LinkButton.x;
            int linky = buttonCurrent.LinkButton.y;
            int dx = buttonEmpty.x - x;
            int dy = buttonEmpty.y - y;

            SMove linkedSwap = new SMoveLinkedSwap(x, y, buttonEmpty.x, buttonEmpty.y, linkx, linky,
                                                    buttonCurrent.islandId, buttonCurrent.LinkButton.islandId);

            Movement movecoords = new Movement(linkx, linky, linkx + dx, linky + dy, buttonCurrent.islandId);

            if (SGrid.Current.CanMove(linkedSwap) && 
                (OpenPath(movecoords) || GetButton(linkx + dx, linky + dy) == buttonCurrent) && 
                moveQueue.Count < maxMoveQueueSize)
            {
                QueueAdd(linkedSwap);

                //L: Swap the current button and the link button
                SwapButtons(buttonCurrent, buttonEmpty);
                SwapButtons(buttonCurrent.LinkButton, GetButton(linkx + dx, linky + dy));
                ProcessQueue();

                return true;
            }
            else
            {
                LogMoveFailure();
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
        int toCheck = Math.Max(Math.Abs(dx), Math.Abs(dy));
        if (dx != 0 && dy != 0)
        {
            return false;
        }
        if (dx == 0)
        {
            int dir = dy / Math.Abs(dy);
            for (int i = 1; i <= toCheck; i++)
            {
                if (GetButton(move.startLoc.x, move.startLoc.y + i * dir).TileIsActive)
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
                if (GetButton(move.startLoc.x + i * dir, move.startLoc.y).TileIsActive)
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// AG: Overriding the UpdatePushedDowns method to get both tiles 2 and 3
    /// pushed down if you press one or the other. Used in event handling in
    /// parent UIArtifact class.
    /// </summary>
    /// <param name="sender">Event handling parameter</param>
    /// <param name="e">Event parameter</param>
    public override void UpdatePushedDowns(object sender, System.EventArgs e)
    {
        foreach (ArtifactTileButton b in _instance.buttons)
        {
            if (b.gameObject.activeSelf)
            {
                if (IsStileInActiveMoves(b.islandId))// || IsStileInQueue(b.islandId))
                {
                    b.SetIsInMove(true);
                }
                else if (b.MyStile.hasAnchor)
                {
                    continue;
                }
                else
                {
                    b.SetIsInMove(false);
                }
            }
        }
    }

    protected override List<ArtifactTileButton> GetMoveOptions(ArtifactTileButton button)
    {
        List<ArtifactTileButton> options = new List<ArtifactTileButton>();

        Vector2Int[] dirs = {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down
        };

        foreach (Vector2Int dir in dirs)
        {
            ArtifactTileButton b = GetButton(button.x + dir.x, button.y + dir.y);
            int i = 2;
            while (b != null && !b.TileIsActive)
            {
                options.Add(b);
                if (button.islandId == 2)
                {
                    // Buttons above and below
                    for (int vert = -2; vert <= 2; vert++)
                    {
                        ArtifactTileButton c = GetButton(button.x + 1, button.y + vert);
                        if (c != null && !c.TileIsActive)
                        {
                            options.Add(c);
                        }
                    }
                    // To the right
                    ArtifactTileButton right = GetButton(button.x + 2, button.y);
                    if (right != null && !right.TileIsActive)
                    {
                        options.Add(right);
                    }
                }
                else if (button.islandId == 3)
                {
                    // Buttons above and below
                    for (int vert = -2; vert <= 2; vert++)
                    {
                        ArtifactTileButton c = GetButton(button.x - 1, button.y + vert);
                        if (c != null && !c.TileIsActive)
                        {
                            options.Add(c);
                        }
                    }
                    // To the left
                    ArtifactTileButton left = GetButton(button.x - 2, button.y);
                    if (left != null && !left.TileIsActive)
                    {
                        options.Add(left);
                    }
                }
                b = GetButton(button.x + dir.x * i, button.y + dir.y * i);

                i++;
            }
        }

        return options;
    }

    
}

