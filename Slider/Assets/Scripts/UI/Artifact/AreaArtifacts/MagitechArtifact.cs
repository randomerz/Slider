using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagitechArtifact : UIArtifact
{
    /*C: Note that this is on the *opposite* side of the grid from the anchor.
    *   IE if the anchor is dropped at (2,1), in the present, this vector will be (5, 1),
    *   the corresponding location in the past, since this is the location we need
    *   to compare against to check for move possibility
    */
    public Vector2Int desynchLocation = new Vector2Int(-1, -1);

    //C: likewise this is the ID of the *opposite* Stile
    public int desynchIslandId = -1; 

    public override void OnEnable()
    {
        base.OnEnable();
        Anchor.OnAnchorInteract += OnAnchorInteract;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        Anchor.OnAnchorInteract -= OnAnchorInteract;
    }

    private void OnAnchorInteract(object sender, Anchor.OnAnchorInteractArgs interactArgs)
    {
        if (interactArgs.drop)
        {
            STile dropTile = interactArgs.stile;
            if(dropTile!= null)
            {
                desynchLocation = new Vector2Int(FindAlt(dropTile.x,3), dropTile.y);
                desynchIslandId = FindAlt(dropTile.islandId, 9);
                //C: If you want to do anything when a desynch starts, do it here, will make a seperate method in the future
            }
        }
        else
        {
            desynchLocation = new Vector2Int(-1, -1);
            desynchIslandId = -1;
            //C: If you want to do anything when a desynch ends, do it here, will make a seperate method in the future
        }
    }

    protected override List<ArtifactTileButton> GetMoveOptions(ArtifactTileButton button)
    {
        moveOptionButtons.Clear();

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
                if(CheckDesynch(button, b)) //C: check for anchor on opposite tile
                    moveOptionButtons.Add(b);
                b = GetButton(button.x + dir.x * i, button.y + dir.y * i);
                i++;
            }
        }

        return moveOptionButtons;
    }

    private bool CheckDesynch(ArtifactTileButton b1, ArtifactTileButton b2)
    {
        if(desynchLocation.x != -1)
        {
            return (b1.islandId == desynchIslandId || !(b2.x == desynchLocation.x && b2.y == desynchLocation.y));
        }
        return true; //C: No desynch active, so valid move
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
        
        SMove swap;
        if(buttonCurrent.islandId == desynchIslandId)
        {
            swap = new SMoveSwap(x, y, buttonEmpty.x, buttonEmpty.y, buttonCurrent.islandId, buttonEmpty.islandId);
            Debug.Log("amogus");
        }
        else
        {
            swap = new SMoveSyncedMove(x, y, buttonEmpty.x, buttonEmpty.y, buttonCurrent.islandId, buttonEmpty.islandId);
        }
       // SMove swap = new SMoveSwap(x, y, buttonEmpty.x, buttonEmpty.y, buttonCurrent.islandId, buttonEmpty.islandId);
        
       // SMove swap2 = new SMoveSwap(FindAlt(x, 3), y, FindAlt(buttonEmpty.x, 3), buttonEmpty.y, FindAlt(buttonCurrent.islandId, 9), FindAlt(buttonEmpty.islandId, 9));

 
        // Debug.Log(SGrid.current.CanMove(swap) + " " + moveQueue.Count + " " + maxMoveQueueSize);
        // Debug.Log(buttonCurrent + " " + buttonEmpty);
        if (SGrid.current.CanMove(swap) && moveQueue.Count < maxMoveQueueSize && PlayerCanQueue)
        {
            //L: Do the move
            MoveMadeOnArtifact?.Invoke(this, null);
            QueueCheckAndAdd(swap);
            //QueueCheckAndAdd(swap2);
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
