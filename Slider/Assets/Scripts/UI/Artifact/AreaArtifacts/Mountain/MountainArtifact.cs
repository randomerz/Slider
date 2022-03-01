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

        foreach (Vector3Int dir in dirs)
        {
            MountainArtifactButton b = GetButton(button.x + dir.x, button.y + dir.y, button.z + dir.z);
            int i = 1;
            while (b != null && !b.isTileActive)
            {
                moveOptionButtons.Add(b);
                b = GetButton(button.x + dir.x * i, button.y + dir.y * i, button.z + dir.z * i);

                i++;
            }
        }

        return moveOptionButtons;
    }

    //L: Swaps the buttons on the UI, but not the actual grid.
    private void SwapButtons(MountainArtifactButton buttonCurrent, MountainArtifactButton buttonEmpty)
    {
        int oldCurrX = buttonCurrent.x;
        int oldCurrY = buttonCurrent.y;
        int oldCurrZ = buttonCurrent.z;
        buttonCurrent.SetPosition(buttonEmpty.x, buttonEmpty.y, buttonEmpty.z);
        buttonEmpty.SetPosition(oldCurrX, oldCurrY, oldCurrZ);
    }

    //L: updateGrid - if this is false, it will just update the UI without actually moving the tiles.
    //L: Returns if the swap was successful.
    private bool CheckAndSwap(MountainArtifactButton buttonCurrent, MountainArtifactButton buttonEmpty)
    {
        STile[,] currGrid = SGrid.current.GetGrid();

        int x = buttonCurrent.x;
        int y = buttonCurrent.y;
        int z = buttonCurrent.z;
        SMove swap = new SMoveSwap(x, y, buttonEmpty.x, buttonEmpty.y);
 
        // Debug.Log(SGrid.current.CanMove(swap) + " " + moveQueue.Count + " " + maxMoveQueueSize);
        // Debug.Log(buttonCurrent + " " + buttonEmpty);
        if (SGrid.current.CanMove(swap) && moveQueue.Count < maxMoveQueueSize)
        {
            //L: Do the move

            QueueCheckAndAdd(new SMoveSwap(buttonCurrent.x, buttonCurrent.y, buttonEmpty.x, buttonEmpty.y));
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

    public static void SetButtonPos(int islandId, int x, int y, int z)
    {
        foreach (MountainArtifactButton b in _instance.buttons)
        {
            if (b.islandId == islandId)
            {
                b.SetPosition(x, y, z);
                return;
            }
        }
    }

    private MountainArtifactButton GetButton(int x, int y, int z)
    {
        foreach (MountainArtifactButton b in _instance.buttons)
        {
            if (b.x == x && b.y == y && b.z == z)
            {
                return b;
            }
        }

        return null;
    }
}
