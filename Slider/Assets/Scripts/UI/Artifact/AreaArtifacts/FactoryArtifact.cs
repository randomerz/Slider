using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//L: Used to handle queue interrupts with conveyors and separate process queue things.
public class FactoryArtifact : UIArtifact
{
    public void QueueMoveToFront(SMove move)
    {
        //L: Conveyor moves always "Cut the line" per se.
        //Unfortunately you can't queue a move at the front since C# queue is not a dequeue, so we have to do this list conversion instead
        List<SMove> newMoveQueue = new List<SMove>(moveQueue);
        newMoveQueue.Insert(0, move);

        SwapButtonsBasedOnMove(move);

        //L: We also have to make sure the interrupted move does not interfere with any of the subsequent moves that have already been made on the artifact.


        moveQueue = new Queue<SMove>(newMoveQueue);
        ProcessQueue();
    }

    private void SwapButtonsBasedOnMove(SMove move)
    {
        var buttonToNewPos = new Dictionary<ArtifactTileButton, Vector2Int>();
        foreach (Movement m in move.moves)
        {
            ArtifactTileButton b = GetButton(m.startLoc.x, m.startLoc.y);
            buttonToNewPos[b] = m.endLoc;
        }

        foreach (var b in buttonToNewPos.Keys)
        {
            b.SetPosition(buttonToNewPos[b].x, buttonToNewPos[b].y);
        }
    }
}
