using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainArtifact : UIArtifact
{
    

    // replaces adjacentButtons
    protected override List<ArtifactTileButton> GetMoveOptions(ArtifactTileButton button)
    {
        moveOptionButtons.Clear();
        Vector3Int[] dirs;
        if(button.y % 2 == 0 ) {
            dirs = new Vector3Int[5] {
            Vector3Int.right,
            Vector3Int.left,
            Vector3Int.up,
            Vector3Int.up * 2,
            Vector3Int.down * 2
            };
        } else {
            dirs = new Vector3Int[5] {
            Vector3Int.right,
            Vector3Int.left,
            Vector3Int.down,
            Vector3Int.down * 2,
            Vector3Int.up * 2
            };
        }
        
        foreach (Vector3Int dir in dirs)
        {
            ArtifactTileButton b = GetButton(button.x + dir.x, button.y + dir.y);
            if(b != null && !b.isTileActive)
                moveOptionButtons.Add(b);
        }

        return moveOptionButtons;
    }

    protected override bool CheckAndSwap(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        STile[,] currGrid = SGrid.current.GetGrid();

        int x = buttonCurrent.x;
        int y = buttonCurrent.y;
        SMove swap;
        //If swapping layers, the difference in y values will be 2
        if(Mathf.Abs(buttonCurrent.y - buttonEmpty.y) < 2) {
            swap = new SMoveSwap(x, y, buttonEmpty.x, buttonEmpty.y, buttonCurrent.islandId, buttonEmpty.islandId);
        }
        else {
            swap = new SMoveLayerSwap(x, y, buttonEmpty.x, buttonEmpty.y, buttonCurrent.islandId, buttonEmpty.islandId);
        }
 
        if (SGrid.current.CanMove(swap) && moveQueue.Count < maxMoveQueueSize)
        {
            //L: Do the move

            QueueCheckAndAdd(swap);
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

    public void AnchorSwap(STile s1, STile s2)
    {
        //We can't just call CheckandSwap because CanMove will return false due to the anchor
        SMove swap = new SMoveLayerSwap(s1.x, s1.y, s2.x, s2.y, s1.islandId, s2.islandId);
        QueueCheckAndAdd(swap);
        SwapButtons(GetButton(s1.islandId), GetButton(s2.islandId));
        QueueCheckAfterMove(this, null);
    }
}
