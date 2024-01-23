using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactoryArtifact : UIArtifact
{
    [SerializeField] private Image background;
    [SerializeField] private Sprite pastBackgroundSprite;
    [SerializeField] private Sprite presentBackgroundSprite;
    public static bool DequeueLocked = false;

    private new void Awake()
    {
        base.Awake();
    }

    private new void OnEnable()
    {
        FactoryGrid.PlayerChangedTime += PlayerChangedTime;
        UIArtifactMenus.OnArtifactOpened += PlayerChangedTime;
        base.OnEnable();
    }

    private new void OnDisable()
    {
        FactoryGrid.PlayerChangedTime -= PlayerChangedTime;
        UIArtifactMenus.OnArtifactOpened -= PlayerChangedTime;
        base.OnDisable();
    }

    private void PlayerChangedTime(object sender, System.EventArgs e)
    {
        UpdateButtonSpritesAndBackground(FactoryGrid.PlayerInPast);
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
        move.forceFullDuration = true;

        //L: Conveyor moves always "Cut the line" per se.
        //Unfortunately you can't queue a move at the front since C# queue is not a dequeue, so we have to do this list conversion instead
        List<SMove> newMoveQueue = new List<SMove>(moveQueue);
        newMoveQueue.Insert(0, move);

        //L: We also have to make sure the queued move does not interfere with any of the subsequent moves that have already been made on the artifact.
        UndoMovesAfterOverlap(newMoveQueue, move);

        SwapButtonsBasedOnMove(move);

        moveQueue = new Queue<SMove>(newMoveQueue);
        base.ProcessQueue();
    }

    private void UndoMovesAfterOverlap(List<SMove> newMoveQueue, SMove moveToCheck) {
        int cutIndex = newMoveQueue.Count;
        HashSet<ArtifactTileButton> movedButtons = new();
        bool shouldDoEffects = false;

        for (int i = 1; i < newMoveQueue.Count; i++)
        {
            // SMoveConveyor should never interfere, but just in case we don't want them to be undone.
            if (newMoveQueue[i].Overlaps(moveToCheck))
            {
                cutIndex = i;
                break;
            }
        }

        // Undo the moves on the artifact in the reverse order that they were made.
        for (int i = newMoveQueue.Count - 1; i >= cutIndex; i--)
        {
            UndoSwapsBasedOnMove(newMoveQueue[i]);
            newMoveQueue.RemoveAt(i);
            shouldDoEffects = true;
        }

        if (shouldDoEffects)
        {
            // Effects
            AudioManager.Play("Artifact Error");
        }
    }

    private void SwapButtonsBasedOnMove(SMove move)
    {
        Dictionary<ArtifactTileButton, Vector2Int> buttonToNewPos = new();

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
        Dictionary<ArtifactTileButton, Vector2Int> buttonToNewPos = new();
        foreach (Movement m in move.moves)
        {
            ArtifactTileButton b = GetButton(m.endLoc.x, m.endLoc.y);
            buttonToNewPos[b] = m.startLoc;
        }

        foreach (var b in buttonToNewPos.Keys)
        {
            b.SetPosition(buttonToNewPos[b].x, buttonToNewPos[b].y, false);
            
            if (b.TileIsActive)
            {
                b.FlickerImmediate(1);
            }
        }

        UpdateMoveOptions();
    }

    private void UpdateButtonSpritesAndBackground(bool inPast)
    {
        foreach (ArtifactTileButton b in buttons)
        {
            if (inPast)
            {
                b.GetComponent<ArtifactTBPluginPast>().UsePastSprite();
                b.RestoreDefaultEmptySprite();
            } else
            {
                b.RestoreDefaultIslandSprite();
                b.RestoreDefaultCompletedSprite();
                b.GetComponent<ArtifactTBPluginConveyor>().UpdateEmptySprite();
            }

            b.SetSpriteToIslandOrEmpty();
        }
        background.sprite = inPast ? pastBackgroundSprite : presentBackgroundSprite;
    }
}
