using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class UIArtifact : MonoBehaviour
{
    // public Vector3 tempPosition = new Vector3(0,0,0);
    public ArtifactTileButton[] buttons;
    //L: The button the user has clicked on
    protected ArtifactTileButton currentButton;
    //L: The available buttons the player has to move to from currentButton
    protected List<ArtifactTileButton> moveOptionButtons = new List<ArtifactTileButton>();

    //queue is used for when the player makes multiple moves before a move has finished.
    private Queue<SMove> moveQueue;
    public int maxMovesBuffered = 2;

    private static UIArtifact _instance;
    
    public void Awake()
    {
        _instance = this;
        moveQueue = new Queue<SMove>();
    }

    public static UIArtifact GetInstance()
    {
        return _instance;
    }

    //L: Handles when the user attempts to drag and drop a button
    public void ButtonDragged(BaseEventData eventData) {
        // Debug.Log("dragging");
        PointerEventData data = (PointerEventData) eventData;

        if (currentButton != null) 
        {
            return;
        }

        ArtifactTileButton dragged = data.pointerDrag.GetComponent<ArtifactTileButton>();
        if (!dragged.isTileActive || dragged.isForcedDown)
        {
            return;
        }

        ArtifactTileButton hovered = null;
        if (data.pointerEnter != null && data.pointerEnter.name == "Image") 
        {
            hovered = data.pointerEnter.transform.parent.gameObject.GetComponent<ArtifactTileButton>();
        }

        
        foreach (ArtifactTileButton b in GetMoveOptions(dragged)) {
            if(b == hovered) 
            {
                b.buttonAnimator.sliderImage.sprite = b.hoverSprite;
            }
            else 
            {
                b.buttonAnimator.sliderImage.sprite = b.emptySprite;
            }
        }
    }
    public void ButtonDragEnd(BaseEventData eventData) {
        PointerEventData data = (PointerEventData) eventData;
        //Debug.Log("Sent drag end");
        if (currentButton != null) 
        {
            return;
        }

        ArtifactTileButton dragged = data.pointerDrag.GetComponent<ArtifactTileButton>();
        if (!dragged.isTileActive || dragged.isForcedDown)
        {
            return;
        }

        ArtifactTileButton hovered = null;
        if (data.pointerEnter != null && data.pointerEnter.name == "Image") 
        {
            hovered = data.pointerEnter.transform.parent.gameObject.GetComponent<ArtifactTileButton>();
        }
        else 
        {
            return;
        }
        hovered.buttonAnimator.sliderImage.sprite = hovered.emptySprite;
        //Debug.Log("dragged" + dragged.islandId + "hovered" + hovered.islandId);

        foreach (ArtifactTileButton b in GetMoveOptions(dragged)) {
            b.buttonAnimator.sliderImage.sprite = b.emptySprite;
            if(b == hovered) 
            {
                CheckAndSwap(dragged, hovered, false);
            }

        }
    }
    public void OnDisable()
    {
        moveQueue = new Queue<SMove>();
        //Debug.Log("Queue Cleared!");
    }

    public void DeselectCurrentButton()
    {
        if (currentButton == null)
            return;

        currentButton.SetSelected(false);
        foreach (ArtifactTileButton b in moveOptionButtons)
        {
            b.SetHighlighted(false);
        }
        currentButton = null;
        moveOptionButtons.Clear();
    }
    
    public virtual void SelectButton(ArtifactTileButton button)
    {
        // Check if on movement cooldown
        //if (SGrid.GetStile(button.islandId).isMoving)

        //L: This is basically just a bunch of nested logic to determine how to update the UI based on what button the user pressed.

        ArtifactTileButton oldCurrButton = currentButton;
        if (currentButton != null)
        {
            if (moveOptionButtons.Contains(button))
            {
                //L: Player makes a valid move on the artifact
                if (currentButton.isForcedDown)
                {
                    //L: Player makes a move while the tile is still moving, so add the button to the queue.
                    CheckAndSwap(currentButton, button, true);

                    //Debug.Log(currentButton.gameObject.name + " added to the queue!");
                    //Debug.Log(button.gameObject.name + " added to the Queue");
                } else
                {
                    //L: Player makes a move immediately
                    CheckAndSwap(currentButton, button, false);
                }

                moveOptionButtons = GetMoveOptions(currentButton);
                foreach (ArtifactTileButton b in buttons)
                {
                    b.SetHighlighted(moveOptionButtons.Contains(b));
                }
            } else 
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

    // replaces adjacentButtons
    protected List<ArtifactTileButton> GetMoveOptions(ArtifactTileButton button)
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
            while (b != null && !b.isTileActive)
            {
                moveOptionButtons.Add(b);
                b = GetButton(button.x + dir.x * i, button.y + dir.y * i);

                i++;
            }
        }

        return moveOptionButtons;
    }

    //L: Swaps the buttons on the UI, but not the actual grid.
    protected void SwapButtons(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        int oldCurrX = buttonCurrent.x;
        int oldCurrY = buttonCurrent.y;
        buttonCurrent.SetPosition(buttonEmpty.x, buttonEmpty.y);
        buttonEmpty.SetPosition(oldCurrX, oldCurrY);
    }

    //L: updateGrid - if this is false, it will just update the UI without actually moving the tiles.
    //L: Returns if the swap was successful.
    protected virtual bool CheckAndSwap(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty, bool queuedMove)
    {
        STile[,] currGrid = SGrid.current.GetGrid();

        int x = buttonCurrent.x;
        int y = buttonCurrent.y;
        SMove swap = new SMoveSwap(x, y, buttonEmpty.x, buttonEmpty.y);
 
        if (SGrid.current.CanMove(swap))
        {
            //L: If the move is queued (queuedMove) then we need to wait until the move is dequed before doing it to the grid.
            if (queuedMove)
            {
                QueueCheckAndAdd(new SMoveSwap(buttonCurrent.x, buttonCurrent.y, buttonEmpty.x, buttonEmpty.y));
            } else
            {
                SGrid.current.Move(swap);
                StartCoroutine(WaitForMoveThenEmptyQueue(buttonCurrent));
            }
            SwapButtons(buttonCurrent, buttonEmpty);
            return true;
        }
        else
        {
            Debug.Log("Couldn't perform move!");
            return false;
        }
    }

    public void QueueCheckAndAdd(SMove move)
    {
        if (moveQueue.Count < maxMovesBuffered)
        {
            moveQueue.Enqueue(move);
        } else
        {
            Debug.LogWarning("Didn't add to the UIArtifact queue because it was full");
        }

    }

    public bool QueueCheckAndRemove()
    {
        if (moveQueue.Count > 0)
        {
            SMove move = moveQueue.Dequeue();
            //Debug.Log("Swapping " + currentButton.gameObject.name + " with " + emptyButton.gameObject.name);

            //L: Update the grid since the Artifact UI should have already updated.
            //L: This move should have already been checked since it was queued!
            SGrid.current.Move(move);
            return true;
        }

        return false;
    }

    protected IEnumerator WaitForMoveThenEmptyQueue(ArtifactTileButton button)
    {
        button.SetForcedPushedDown(true);

        do
        {
            yield return new WaitForSeconds(1);
        } while (QueueCheckAndRemove());

        button.SetForcedPushedDown(false);
    }

    //public static void UpdatePushedDowns()
    //{
    //    foreach (ArtifactButton b in _instance.buttons)
    //    {
    //        b.UpdatePushedDown();
    //    }
    //}

    //L: Mark the button on the Artifact UI at islandID if it is in the right spot. (also changes the sprite)
    public static void SetButtonComplete(int islandId, bool value)
    {
        foreach (ArtifactTileButton b in _instance.buttons)
        {
            if (b.islandId == islandId)
            {
                b.SetComplete(value);
                return;
            }
        }
    }

    public static void SetButtonPos(int islandId, int x, int y)
    {
        foreach (ArtifactTileButton b in _instance.buttons)
        {
            if (b.islandId == islandId)
            {
                b.SetPosition(x, y);
                return;
            }
        }
    }

    protected ArtifactTileButton GetButton(int x, int y)
    {
        foreach (ArtifactTileButton b in _instance.buttons)
        {
            if (b.x == x && b.y == y)
            {
                return b;
            }
        }

        return null;
    }

    public static void AddButton(int islandId)
    {
        foreach (ArtifactTileButton b in _instance.buttons)
        {
            if (b.islandId == islandId)
            {
                b.SetTileActive(true);
                return;
            }
        }
    }

    public void FlickerNewTiles()
    {
        foreach (ArtifactTileButton b in _instance.buttons)
        {
            if (b.flickerNext)
            {
                b.Flicker();
            }
        }
    }
}
