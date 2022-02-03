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
    
    public override void SelectButton(ArtifactTileButton button)
    {
        //WORK ON THIS TO MAKE MOVE QUEUING WORK
        if (currentButton != null && currentButton.isForcedDown && moveOptionButtons.Contains(button))
        {
            //Debug.Log(currentButton.gameObject.name + " added to the queue!");
            //Debug.Log(button.gameObject.name + " added to the Queue");
            //QueueCheckAndAdd(currentButton, button);
            DeselectCurrentButton();
        }

        else if (currentButton == button)
        {
            DeselectCurrentButton();
        }
        else if (moveOptionButtons.Contains(button))
        {
            //Compare by tile location to determine direction of sliding
            int x_dif = button.x - currentButton.x;
            int y_dif = button.y - currentButton.y;

            if (x_dif > 0) { MoveRight(); }
            else if (x_dif < 0) { MoveLeft(); }
            else if (y_dif > 0) { MoveUp(); }
            else { MoveDown();}

            DeselectCurrentButton();
        }
        else
        {
            DeselectCurrentButton();

            if (!button.isTileActive)
            {
                return;
            }

            moveOptionButtons = GetMoveOptions(button);
            if (moveOptionButtons.Count == 0)
            {
                return;
            }
            else
            {
                //Debug.Log("Selected button " + button.islandId);
                currentButton = button;
                button.SetPushedDown(true);
                foreach (ArtifactTileButton b in moveOptionButtons)
                {
                    b.SetHighlighted(true);
                }
            }
        }
        //else
        //{
        //    // default deselect
        //    DeselectCurrentButton();
        //}
    }
    //Chen: Below are the 4 methods for sliding all tiles right, up, left, or down.
    public void MoveRight()
    {
        for (int row = 1; row >= 0; row--)
        {
            List<ArtifactTileButton> tiles = new List<ArtifactTileButton>
            {
            GetButton(row, 0),
            GetButton(row, 1),
            GetButton(row, 2)
            };
            
            foreach (ArtifactTileButton button in tiles)
            {
                //Similar to getAdjacent()
                if (button.isTileActive)
                {
                    ArtifactTileButton curr = button;
                    ArtifactTileButton last = null;
                    Vector2Int dir = Vector2Int.right;
                    for (int i = 1; i < 3; i++)
                    {
                        curr = GetButton(button.x + dir.x * i, button.y + dir.y * i);
                        if (curr == null || curr.isTileActive)
                        {
                            break;
                        }
                        else if (curr != null && !curr.isTileActive)
                        {
                            last = curr;
                        }
                    }
                    if (last != null)
                    {
                        CheckAndSwap(button, last);
                    }
                }
            }
        }
    }

    public void MoveLeft()
    {
        for (int row = 1; row < 3; row++)
        {
            List<ArtifactTileButton> tiles = new List<ArtifactTileButton>
            {
            GetButton(row, 0),
            GetButton(row, 1),
            GetButton(row, 2)
            };
            foreach (ArtifactTileButton button in tiles)
            {
                if (button.isTileActive)
                {
                    //Similar to getMoveOptions
                    ArtifactTileButton curr = button;
                    ArtifactTileButton last = null;
                    Vector2Int dir = Vector2Int.left;
                    for (int i = 1; i < 3; i++)
                    {
                        curr = GetButton(button.x + dir.x * i, button.y + dir.y * i);
                        if (curr != null && !curr.isTileActive)
                        {
                            last = curr;
                        }
                        else if (curr == null || curr.isTileActive)
                        {
                            break;
                        }
                    }
                    if (last != null)
                    {
                        CheckAndSwap(button, last);
                    }
                }
            }
        }
    }

    public void MoveUp()
    {
        for (int col = 1; col >= 0; col--)
        {
            List<ArtifactTileButton> tiles = new List<ArtifactTileButton>
            {
            GetButton(0, col),
            GetButton(1, col),
            GetButton(2, col)
            };
            foreach (ArtifactTileButton button in tiles)
            {
                if (button.isTileActive)
                {
                    //Similar to getAdjacent()
                    ArtifactTileButton curr = button;
                    ArtifactTileButton last = null;
                    Vector2Int dir = Vector2Int.up;
                    for (int i = 1; i < 3; i++)
                    {
                        curr = GetButton(button.x + dir.x * i, button.y + dir.y * i);
                        if (curr != null && !curr.isTileActive)
                        {
                            last = curr;
                        }
                        else if (curr == null || curr.isTileActive)
                        {
                            break;
                        }
                    }
                    if (last != null)
                    {
                        CheckAndSwap(button, last);
                    }
                }
            }
        }
    }

    public void MoveDown()
    {
        for (int col = 1; col < 3; col++)
        {
            List<ArtifactTileButton> tiles = new List<ArtifactTileButton>
            {
            GetButton(0, col),
            GetButton(1, col),
            GetButton(2, col)
            };
            foreach (ArtifactTileButton button in tiles)
            {
                if (button.isTileActive)
                {
                    //Similar to getAdjacent()
                    ArtifactTileButton curr = button;
                    ArtifactTileButton last = null;
                    Vector2Int dir = Vector2Int.down;
                    for (int i = 1; i < 3; i++)
                    {
                        curr = GetButton(button.x + dir.x * i, button.y + dir.y * i);
                        if (curr != null && !curr.isTileActive)
                        {
                            last = curr;
                        }
                        else if (curr == null || curr.isTileActive)
                        {
                            break;
                        }
                    }
                    if (last != null)
                    {
                        CheckAndSwap(button, last);
                    }
                }
            }
        }
            
    }
}
