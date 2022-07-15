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
        //Vector2 buttPos = new Vector2(button.x, button.y);
        // foreach (ArtifactTileButton b in buttons)
        // {
        //     //if (!b.isTileActive && (buttPos - new Vector2(b.x, b.y)).magnitude == 1)
        //     if (!b.isTileActive && (button.x == b.x || button.y == b.y))
        //     {
        //         adjacentButtons.Add(b);
        //     }
        // }

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

    //Chen: TryQueueMoveFromButtonPair now calls each of the Slide() functions
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
        SetButtonPositionsToMatchGrid();
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

            UpdateSwaps(swaps, tiles, Vector2Int.right);
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
            UpdateSwaps(swaps, tiles, Vector2Int.left);
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
            UpdateSwaps(swaps, tiles, Vector2Int.up);
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
            UpdateSwaps(swaps, tiles, Vector2Int.down);
        }
        return new SSlideSwap(swaps);
    }

    private void UpdateSwaps(List<Movement> swaps, List<ArtifactTileButton> tiles, Vector2Int dir)
    {
        Vector2Int lastSwap = new Vector2Int(-1, -1);
        Vector2Int firstSwap = new Vector2Int(-1, -1);
        int firstIslandId = -1;

        foreach (ArtifactTileButton button in tiles)
        {
            if (button.TileIsActive)
            {
                ArtifactTileButton furthest = GetLastEmpty(button, dir);
                if (furthest != null)
                {
                    lastSwap = new Vector2Int(button.x, button.y);
                    if (firstSwap.x == -1)
                    {
                        firstSwap = new Vector2Int(furthest.x, furthest.y);
                        firstIslandId = furthest.islandId;
                    }
                    swaps.Add(new Movement(button.x, button.y, furthest.x, furthest.y, button.islandId));
                    //SwapButtons(button, furthest);  //L: Elliot, I love you, but this is a cardinal sin.
                }
            }
        }
        if (lastSwap.x != -1 && firstSwap.x != -1)
        {
            //print(firstSwap.x);
            swaps.Add(new Movement(firstSwap.x, firstSwap.y, lastSwap.x, lastSwap.y, firstIslandId));
        }
    }

    //Chen: finds the furthest button a given button can go to given a direction
    private ArtifactTileButton GetLastEmpty(ArtifactTileButton button, Vector2Int dir)
    {

        ArtifactTileButton curr = GetButton(button.x + dir.x, button.y + dir.y);
        //ArtifactTileButton next = GetButton(curr.x + dir.x, curr.y + dir.y);
        ArtifactTileButton last = null;

        //Anchor Case: Don't move if the tile has anchor or if it's blocked by an anchor
        STile[,] grid = SGrid.Current.GetGrid();
        if (grid[button.x, button.y].hasAnchor || grid[curr.x, curr.y].hasAnchor)
        {
            return null;
        }
        for (int i = 0; i < 2; i++)
        {
            if (curr != null)
            {
                //Case 1: Tile slides 2 spaces
                if (!curr.TileIsActive)
                {
                    last = curr;
                } //Edge Case: A tile on the edge will slide to the middle
                else if (GetButton(curr.x + dir.x, curr.y + dir.y) != null && !GetButton(curr.x + dir.x, curr.y + dir.y).TileIsActive)
                {
                    last = curr;
                    break;
                }
            }
            else
            {
                break;
            }
            curr = GetButton(curr.x + dir.x, curr.y + dir.y);
        }
        // Debug.Log(last);
        return last;
    }
    #endregion
}
