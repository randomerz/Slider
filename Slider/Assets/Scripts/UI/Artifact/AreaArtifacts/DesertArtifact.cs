using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DesertArtifact : UIArtifact
{
    //Chen: getMoveOptions will add buttons even if they're active for Desert sliding
    protected override List<ArtifactTileButton> GetMoveOptions(ArtifactTileButton button)
    {
        var options = new List<ArtifactTileButton>();
        if (button == null)
        {
            return options;
        }

        Vector2Int[] dirs = {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down
        };

        foreach (Vector2Int dir in dirs)
        {
            ArtifactTileButton b = GetButton(button.x + dir.x, button.y + dir.y);
            int i = 2;  //i=1 is checked in the above line, otherwise it will add the same option twice.
            while (b != null)
            {
                options.Add(b);
                b = GetButton(button.x + dir.x * i, button.y + dir.y * i);

                i++;
            }
        }

        return options;
    }

    //Chen: Override for dragndrop since desert GetMoveOPtions include  active tiles
    //L: Deleted ButtonDragEnd override because the code was exactly the same and GetMoveOptions is marked virtual so it will automatically call the right one.

    public override bool TryQueueMoveFromButtonPair(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        SMove move;

        if (moveQueue.Count < maxMoveQueueSize)
        {
            move = ConstructMoveFromButtonPair(buttonCurrent, buttonEmpty);
            if (move.moves.Count == 0)
            {
                return false;
            }
            QueueMoveFromButtonPair(move, buttonCurrent, buttonEmpty);
            return true;
        }
        else
        {
            LogMoveFailure();
            return false;
        }
    }

    protected override void QueueMoveFromButtonPair(SMove move, ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        MoveMadeOnArtifact?.Invoke(this, null);
        QueueAdd(move);
        ProcessQueue();
        DeselectSelectedButton();
        UpdatePushedDowns(null, null);
    }

    protected override SMove ConstructMoveFromButtonPair(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        //Chen: Nested logic pain time.
        SSlideSwap move;
        int dx = buttonEmpty.x - buttonCurrent.x;
        int dy = buttonEmpty.y - buttonCurrent.y;
        if (dx > 0)
        {
            move = SlideRight();
        }
        else if (dx < 0)
        {
            move = SlideLeft();
        }
        else if (dy > 0)
        {
            move = SlideUp();
        }
        else
        {
            move = SlideDown();
        }

        return move;
    }

    #region SSlideSwap
    //Chen: Below are the 4 methods for sliding all tiles. UI swapping is handled here
    public SSlideSwap SlideRight()
    {
        List<Movement> swaps = new List<Movement>();
        
        for (int col = 0; col < 3; col++)
        {
            List<ArtifactTileButton> tiles = new List<ArtifactTileButton>
            {
            GetButton(1, col),
            GetButton(0, col)
            };

            UpdateSwapsAndUI(swaps, tiles, Vector2Int.right);
        }
        return new SSlideSwap(swaps);
    }

    public SSlideSwap SlideLeft()
    {
        List<Movement> swaps = new List<Movement>();

        for (int col = 0; col < 3; col++)
        {
            List<ArtifactTileButton> tiles = new List<ArtifactTileButton>
            {
            GetButton(1, col),
            GetButton(2, col)
            };
            UpdateSwapsAndUI(swaps, tiles, Vector2Int.left);
        }
        return new SSlideSwap(swaps);
    }

    public SSlideSwap SlideUp()
    {
        List<Movement> swaps = new List<Movement>();

        for (int row = 0; row < 3; row++)
        {
            List<ArtifactTileButton> tiles = new List<ArtifactTileButton>
            {
            GetButton(row, 1),
            GetButton(row, 0)
            };
            UpdateSwapsAndUI(swaps, tiles, Vector2Int.up);
        }
        return new SSlideSwap(swaps);
    }

    public SSlideSwap SlideDown()
    {
        List<Movement> swaps = new List<Movement>();

        for (int row = 0; row < 3; row++)
        {
            List<ArtifactTileButton> tiles = new List<ArtifactTileButton>
            {
            GetButton(row, 1),
            GetButton(row, 2),
            };
            UpdateSwapsAndUI(swaps, tiles, Vector2Int.down);
        }
        return new SSlideSwap(swaps);
    }

    private void UpdateSwapsAndUI(List<Movement> swaps, List<ArtifactTileButton> tiles, Vector2Int dir)
    {
        Vector2Int emptyButtonStart = new Vector2Int(-1, -1);
        Vector2Int emptyButtonEnd = new Vector2Int(-1, -1);
        int emptyIslandId = -1;

        foreach (ArtifactTileButton button in tiles)
        {
            if (button.TileIsActive)
            {
                ArtifactTileButton furthest = GetLastEmpty(button, dir);
                if (furthest != null)
                {
                    if (emptyButtonStart.x == -1)
                    {
                        emptyButtonStart = new Vector2Int(furthest.x, furthest.y);
                        emptyIslandId = furthest.islandId;
                    }
                    emptyButtonEnd = new Vector2Int(button.x, button.y);


                    swaps.Add(new Movement(button.x, button.y, furthest.x, furthest.y, button.islandId));
                    SwapButtons(button, furthest);  //L: This is kinda bad to do here since it's a side effect of constructing the move, but I don't want to break it (since I already did)
                }
            }
        }
        if (emptyButtonStart.x != -1 && emptyButtonEnd.x != -1)
        {
            swaps.Add(new Movement(emptyButtonStart.x, emptyButtonStart.y, emptyButtonEnd.x, emptyButtonEnd.y, emptyIslandId));
        }
    }

    //Chen: finds the furthest button a given button can go to given a direction
    private ArtifactTileButton GetLastEmpty(ArtifactTileButton button, Vector2Int dir)
    {

        ArtifactTileButton curr = GetButton(button.x + dir.x, button.y + dir.y);
        ArtifactTileButton furthest = null;

        //Anchor Case: Don't move if the tile has anchor or if it's blocked by an anchor
        STile[,] grid = SGrid.Current.GetGrid();
        if (grid[button.x, button.y].hasAnchor || grid[curr.x, curr.y].hasAnchor)
        {
            return null;
        }

        while (curr != null)
        {
            if (!curr.TileIsActive)
            {
                //Case 1: Tile slides to the empty space
                furthest = curr;
            } 

            curr = GetButton(curr.x + dir.x, curr.y + dir.y);
        }
        return furthest;
    }
    #endregion
}
