using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertArtifact : UIArtifact
{
    protected new Queue<SSlideSwap> moveQueue;

    public new void Awake()
    {
        _instance = this;
        moveQueue = new Queue<SSlideSwap>();
    }

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


        for (int i = 0; i < 2; i++)
        {
            if (curr != null)
            {
                //Case 1: Tile slides 2 spaces
                if (!curr.isTileActive)
                {
                    last = curr;
                } 
                else if (GetButton(curr.x + dir.x, curr.y + dir.y) != null && !GetButton(curr.x + dir.x, curr.y + dir.y)) {
                    last = curr;
                    break;
                }
                //Edge Case: A tile on the edge will slide to the middle

            }
            else
            {
                break;
            }
            curr = GetButton(curr.x + dir.x, curr.y + dir.y);
        }
        //Debug.Log(last);
        return last;
    }
    /*
    public override void SelectButton(ArtifactTileButton button)
    {
        // Check if on movement cooldown
        //if (SGrid.GetStile(button.islandId).isMoving)

        //L: This is basically just a bunch of nested logic to determine how to update the UI based on what button the user pressed.

        ArtifactTileButton oldCurrButton = currentButton;
        if (currentButton != null)
        {
            if (moveOptionButtons.Contains(button))
            {

                //L: Player makes a move while the tile is still moving, so add the button to the queue.
                CheckAndSwap(currentButton, button);

                moveOptionButtons = GetMoveOptions(currentButton);
                foreach (ArtifactTileButton b in buttons)
                {
                    b.SetHighlighted(moveOptionButtons.Contains(b));
                }
            }
            else
            {
                DeselectCurrentButton();
            }
        }

        if (currentButton == null)
        {
            //DeselectCurrentButton(); //L: I don't think this is necessary since currentButton is null and it will just do nothing

            if (!button.isTileActive || oldCurrButton == button)
            {
                //L: Player tried to click an empty tile
                return;
            }

            moveOptionButtons = GetMoveOptions(button);
            if (moveOptionButtons.Count == 0)
            {
                //L: Player tried to click a locked tile (or tile that otherwise had no move options)
                return;
            }
            else
            {
                //L: Player clicked a tile with movement options
                //Debug.Log("Selected button " + button.islandId);
                currentButton = button;
                button.SetSelected(true);
                foreach (ArtifactTileButton b in moveOptionButtons)
                {
                    b.SetHighlighted(true);
                }
            }
        }
    }
    */
    //L: updateGrid - if this is false, it will just update the UI without actually moving the tiles.
    //L: Returns if the swap was successful.
    //Chen: CheckAndSwap now calls each of the Slide() functions
    protected override bool CheckAndSwap(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        STile[,] currGrid = SGrid.current.GetGrid();

        int dx = buttonEmpty.x - buttonCurrent.x;
        int dy = buttonEmpty.y - buttonCurrent.y;
        SSlideSwap swap;

        //Nested logic pain time
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

        if (SGrid.current.CanMove(swap))
        {
            //L: Do the move

            SlideQueueCheckAndAdd(swap);

            if (moveQueue.Count == 1)
            {
                SGrid.current.Move(moveQueue.Peek());
            }
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
            Debug.LogWarning(moveQueue.Count);
            Debug.LogWarning("Didn't add to the UIArtifact queue because it was full");
        }

    }

    //Chen: Below are the 4 methods for sliding all tiles. UI swapping is handled here
    public SSlideSwap SlideRight()
    {
        List<Vector4Int> swaps = new List<Vector4Int>();

        for (int row = 1; row >= 0; row--)
        {
            List<ArtifactTileButton> tiles = new List<ArtifactTileButton>
            {
            GetButton(row, 0),
            GetButton(row, 1),
            GetButton(row, 2)
            };
            
            //Chen: For each active tile in a specific order, get the furthest empty tile and add to list of swaps for the slide
            foreach (ArtifactTileButton button in tiles)
            {
                if (button.isTileActive)
                {
                    ArtifactTileButton last = GetLastEmpty(button, Vector2Int.right);
                    if (last != null)
                    {
                        swaps.Add(new Vector4Int(button.x, button.y, last.x, last.y));
                        SwapButtons(button, last);
                    }
                }
            }
        }
        return new SSlideSwap(swaps);
    }

    public SSlideSwap SlideLeft()
    {
        List<Vector4Int> swaps = new List<Vector4Int>();

        for (int row = 1; row < 3; row++)
        {
            List<ArtifactTileButton> tiles = new List<ArtifactTileButton>
            {
            GetButton(row, 0),
            GetButton(row, 1),
            GetButton(row, 2)
            };

            //Chen: For each active tile in a specific order, get the furthest empty tile and add to list of swaps for the slide
            foreach (ArtifactTileButton button in tiles)
            {
                if (button.isTileActive)
                {
                    ArtifactTileButton last = GetLastEmpty(button, Vector2Int.left);
                    if (last != null)
                    {
                        swaps.Add(new Vector4Int(button.x, button.y, last.x, last.y));
                        SwapButtons(button, last);
                    }
                }
            }
        }
        return new SSlideSwap(swaps);
    }

    public SSlideSwap SlideUp()
    {
        List<Vector4Int> swaps = new List<Vector4Int>();

        for (int col = 1; col >= 0; col--)
        {
            List<ArtifactTileButton> tiles = new List<ArtifactTileButton>
            {
            GetButton(0, col),
            GetButton(1, col),
            GetButton(2, col)
            };

            //Chen: For each active tile in a specific order, get the furthest empty tile and add to list of swaps for the slide
            foreach (ArtifactTileButton button in tiles)
            {
                if (button.isTileActive)
                {
                    ArtifactTileButton last = GetLastEmpty(button, Vector2Int.up);
                    if (last != null)
                    {
                        swaps.Add(new Vector4Int(button.x, button.y, last.x, last.y));
                        SwapButtons(button, last);
                    }
                }
            }
        }
        return new SSlideSwap(swaps);
    }

    public SSlideSwap SlideDown()
    {
        List<Vector4Int> swaps = new List<Vector4Int>();

        for (int col = 1; col < 3; col++)
        {
            List<ArtifactTileButton> tiles = new List<ArtifactTileButton>
            {
            GetButton(0, col),
            GetButton(1, col),
            GetButton(2, col)
            };

            //Chen: For each active tile in a specific order, get the furthest empty tile and add to list of swaps for the slide
            foreach (ArtifactTileButton button in tiles)
            {
                if (button.isTileActive)
                {
                    ArtifactTileButton last = GetLastEmpty(button, Vector2Int.down);
                    if (last != null)
                    {
                        swaps.Add(new Vector4Int(button.x, button.y, last.x, last.y));
                        SwapButtons(button, last);
                    }
                }
            }
        }
        return new SSlideSwap(swaps);
    }
}
