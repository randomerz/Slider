using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertArtifact : UIArtifact
{
    //Chen: getMoveOptions will add buttons even if they're active for Desert sliding
    protected override List<ArtifactTileButton> GetMoveOptions(ArtifactTileButton button)
    {
        moveOptionButtons.Clear();

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
            int i = 1;
            while (b != null)
            {
                moveOptionButtons.Add(b);
                b = GetButton(button.x + dir.x * i, button.y + dir.y * i);

                i++;
            }
        }

        return moveOptionButtons;
    }

    //Chen: finds the furthest button a given button can go to given a direction
    private ArtifactTileButton GetLastEmpty(ArtifactTileButton button, Vector2Int dir)
    {
        
        ArtifactTileButton curr = GetButton(button.x + dir.x, button.y + dir.y);
        //ArtifactTileButton next = GetButton(curr.x + dir.x, curr.y + dir.y);
        ArtifactTileButton last = null;

        //Anchor Case: Don't move if the tile has anchor or if it's blocked by an anchor
        STile[,] grid = SGrid.current.GetGrid();
        if (grid[button.x, button.y].hasAnchor || grid[curr.x, curr.y].hasAnchor)
        {
            return null;
        }
        for (int i = 0; i < 2; i++)
        {
            if (curr != null)
            {
                //Case 1: Tile slides 2 spaces
                if (!curr.isTileActive)
                {
                    last = curr;
                } //Edge Case: A tile on the edge will slide to the middle
                else if (GetButton(curr.x + dir.x, curr.y + dir.y) != null && !GetButton(curr.x + dir.x, curr.y + dir.y).isTileActive) {
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

    //Chen: Overriding the drag and drop calls so sliding works
   
    //L: updateGrid - if this is false, it will just update the UI without actually moving the tiles.
    //L: Returns if the swap was successful.
    //Chen: CheckAndSwap now calls each of the Slide() functions
    protected override bool CheckAndSwap(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        STile[,] currGrid = SGrid.current.GetGrid();

        int dx = buttonEmpty.x - buttonCurrent.x;
        int dy = buttonEmpty.y - buttonCurrent.y;
        SSlideSwap swap;
        //Chen: Check to see if we can add a move. Nested logic pain time.
        if (moveQueue.Count < maxMoveQueueSize)
        {
            
            if (dx > 0)
            {
                swap = SlideRight();
            }
            else if (dx < 0)
            {
                swap = SlideLeft();
            }
            else if (dy > 0)
            {
                swap = SlideUp();
            }
            else
            {
                swap = SlideDown();
            }
            //Chen: If the returned swap has nothing in it (tiles won't move) return false before it can be added.
            if (swap.moves.Count == 0)
            {
                return false;
            }
            //Debug.Log(SGrid.current.CanMove(swap) + " " + moveQueue.Count + " " + maxMoveQueueSize);
            //Debug.Log(buttonCurrent + " " + buttonEmpty);
            QueueCheckAndAdd(swap);
            //Debug.Log("Added move to queue: current length " + moveQueue.Count);
            QueueCheckAfterMove(this, null);
            return true;
        }
        else
        {
            Debug.Log("Couldn't perform move! (queue full?)");
            return false;
        }
    }

    public void SlideQueueCheckAndAdd(SSlideSwap move)
    {
        Debug.Log("Desert Hah scrub");
        if (moveQueue.Count < maxMoveQueueSize)
        {
            moveQueue.Enqueue(move);
        }
        else
        {
            Debug.LogWarning("Didn't add to the UIArtifact queue because it was full");
        }

    }

    private List<Movement> GetSlideMoves(List<Movement> swaps, List<ArtifactTileButton> tiles, Vector2Int dir)
    {
        Vector2Int lastSwap = new Vector2Int(-1, -1);
        Vector2Int firstSwap = new Vector2Int(-1, -1);
        int firstIslandId = -1;

        foreach (ArtifactTileButton button in tiles)
        {
            if (button.isTileActive)
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
                    SwapButtons(button, furthest);
                }
            }
        }
        if (lastSwap.x != -1 && firstSwap.x != -1)
        {
            //print(firstSwap.x);
            swaps.Add(new Movement(firstSwap.x, firstSwap.y, lastSwap.x, lastSwap.y, firstIslandId));
        }

        return swaps;
    }

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

            GetSlideMoves(swaps, tiles, Vector2Int.right);
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
            GetSlideMoves(swaps, tiles, Vector2Int.left);
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
            GetSlideMoves(swaps, tiles, Vector2Int.up);
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
            GetSlideMoves(swaps, tiles, Vector2Int.down);
        }
        return new SSlideSwap(swaps);
    }
}
