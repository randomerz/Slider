using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagitechArtifact : UIArtifact
{
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
            while (b != null && !b.isTileActive && (b.x/3 == button.x/3) ) //C: check that x/3 is the same to not count moves on the other side of the grid as valid
            {
                moveOptionButtons.Add(b);
                b = GetButton(button.x + dir.x * i, button.y + dir.y * i);
                i++;
            }
        }

        return moveOptionButtons;
    }

    //C: basically just modulus. Used to find corresponding values on either side of the grid
    private int FindAlt(int num, int offset)
    {
        return (num + offset) % (offset * 2);
    }


    protected override bool CheckAndSwap(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        STile[,] currGrid = SGrid.current.GetGrid();

        int x = buttonCurrent.x;
        int y = buttonCurrent.y;
        SMove swap = new SMoveSwap(x, y, buttonEmpty.x, buttonEmpty.y, buttonCurrent.islandId, buttonEmpty.islandId);
        
        //C: Don't worry about desynch at the moment
        SMove swap2 = new SMoveSwap(FindAlt(x, 3), y, FindAlt(buttonEmpty.x, 3), buttonEmpty.y, FindAlt(buttonCurrent.islandId, 9), FindAlt(buttonEmpty.islandId, 9));

 
        // Debug.Log(SGrid.current.CanMove(swap) + " " + moveQueue.Count + " " + maxMoveQueueSize);
        // Debug.Log(buttonCurrent + " " + buttonEmpty);
        if (SGrid.current.CanMove(swap) && moveQueue.Count < maxMoveQueueSize && PlayerCanQueue)
        {
            //L: Do the move
            MoveMadeOnArtifact?.Invoke(this, null);
            QueueCheckAndAdd(swap);
            QueueCheckAndAdd(swap2);
            SwapButtons(buttonCurrent, buttonEmpty);

            // Debug.Log("Added move to queue: current length " + moveQueue.Count);
            QueueCheckAfterMove(this, null);
            // if (moveQueue.Count == 1)
            // {
            //     SGrid.current.Move(moveQueue.Peek());
            // }
            return true;
        }
        else
        {
            string debug = PlayerCanQueue ? "Player Queueing is disabled" : "Queue was full";
            Debug.Log($"Couldn't perform move! {debug}");
            return false;
        }
    }
}
