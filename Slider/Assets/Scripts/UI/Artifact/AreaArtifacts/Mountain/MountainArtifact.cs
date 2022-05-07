using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainArtifact : UIArtifact
{
    

    // replaces adjacentButtons
    private List<ArtifactTileButton> GetMoveOptions(MountainArtifactButton button)
    {
        moveOptionButtons.Clear();

        Vector3Int[] dirs = {
            Vector3Int.right,
            Vector3Int.up,
            Vector3Int.left,
            Vector3Int.down,
            Vector3Int.forward,
            Vector3Int.back
        };

       /* foreach (Vector3Int dir in dirs)
        {
            MountainArtifactButton b = GetButton(button.x + dir.x, button.y + dir.y, button.z + dir.z);
            int i = 1;
            while (b != null && !b.isTileActive)
            {
                moveOptionButtons.Add(b);
                b = GetButton(button.x + dir.x * i, button.y + dir.y * i, button.z + dir.z * i);

                i++;
            }
        }*/

        return moveOptionButtons;
    }

    //L: Swaps the buttons on the UI, but not the actual grid.
    private void SwapButtons(MountainArtifactButton buttonCurrent, MountainArtifactButton buttonEmpty)
    {
        int oldCurrX = buttonCurrent.x;
        int oldCurrY = buttonCurrent.y;
        buttonCurrent.SetPosition(buttonEmpty.x, buttonEmpty.y);
        buttonEmpty.SetPosition(oldCurrX, oldCurrY);
    }

    //L: updateGrid - if this is false, it will just update the UI without actually moving the tiles.
    //L: Returns if the swap was successful.
    private bool CheckAndSwap(MountainArtifactButton buttonCurrent, MountainArtifactButton buttonEmpty)
    {
        STile[,] currGrid = SGrid.current.GetGrid();

        int x = buttonCurrent.x;
        int y = buttonCurrent.y;
        SMove swap = new SMoveSwap(x, y, buttonEmpty.x, buttonEmpty.y, buttonCurrent.islandId, buttonEmpty.islandId);
 
        // Debug.Log(SGrid.current.CanMove(swap) + " " + moveQueue.Count + " " + maxMoveQueueSize);
        // Debug.Log(buttonCurrent + " " + buttonEmpty);
        if (SGrid.current.CanMove(swap) && moveQueue.Count < maxMoveQueueSize)
        {
            //L: Do the move

            QueueCheckAndAdd(new SMoveSwap(buttonCurrent.x, buttonCurrent.y, buttonEmpty.x, buttonEmpty.y, buttonCurrent.islandId, buttonEmpty.islandId));
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
            Debug.Log("Couldn't perform move! (queue full?)");
            return false;
        }
    }
}
