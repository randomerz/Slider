using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//L: Used to handle queue interrupts with conveyors and separate process queue things.
public class FactoryArtifact : UIArtifact
{
    public static bool DequeueLocked = false;

    private new void Awake()
    {
        base.Awake();
    }

    public override void ProcessQueue()
    {
        if (!DequeueLocked)
        {
            base.ProcessQueue();
        }
    }

    public void QueueMoveToFront(SMove move)
    {
        //L: Conveyor moves always "Cut the line" per se.
        //Unfortunately you can't queue a move at the front since C# queue is not a dequeue, so we have to do this list conversion instead
        List<SMove> newMoveQueue = new List<SMove>(moveQueue);
        newMoveQueue.Insert(0, move);   //This is inefficient, but like, there's 3 elements in the list.

        //L: We also have to make sure the interrupted move does not interfere with any of the subsequent moves that have already been made on the artifact.
        UndoMovesAfterOverlap(newMoveQueue, move);

        SwapButtonsBasedOnMove(move);


        moveQueue = new Queue<SMove>(newMoveQueue);
        base.ProcessQueue();
    }

    private void UndoMovesAfterOverlap(List<SMove> newMoveQueue, SMove moveToCheck) {
        int cutIndex = newMoveQueue.Count;
        for (int i = 1; i < newMoveQueue.Count; i++)
        {
            //SMoveConveyor should never interfere, but just in case we don't want them to be undone.
            if (!(newMoveQueue[i] is SMoveConveyor) && newMoveQueue[i].Overlaps(moveToCheck))
            {
                cutIndex = i;
                break;
            }
        }

        //Undo the moves on the artifact in the reverse order that they were made.
        for (int i = newMoveQueue.Count-1; i >= cutIndex; i--)
        {
            UndoSwapsBasedOnMove(newMoveQueue[i]);
            newMoveQueue.RemoveAt(i);
        }
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
            b.SetPosition(buttonToNewPos[b].x, buttonToNewPos[b].y, true);
        }

        UpdateMoveOptions();
    }

    private void UndoSwapsBasedOnMove(SMove move)
    {
        var buttonToNewPos = new Dictionary<ArtifactTileButton, Vector2Int>();
        foreach (Movement m in move.moves)
        {
            ArtifactTileButton b = GetButton(m.endLoc.x, m.endLoc.y);
            buttonToNewPos[b] = m.startLoc;
        }

        foreach (var b in buttonToNewPos.Keys)
        {
            if (b.isActiveAndEnabled)   //Can't start coroutines on inactive button.
            {
                b.FlickerImmediate(1);
            }
            b.SetPosition(buttonToNewPos[b].x, buttonToNewPos[b].y, false);
        }

        UpdateMoveOptions();
    }
}
