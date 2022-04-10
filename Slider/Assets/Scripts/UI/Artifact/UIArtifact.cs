using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class UIArtifact : MonoBehaviour
{
    public static System.EventHandler<System.EventArgs> OnButtonInteract;

    public ArtifactTileButton[] buttons;
    //L: The button the user has clicked on
    protected ArtifactTileButton currentButton;
    //L: The available buttons the player has to move to from currentButton
    protected List<ArtifactTileButton> moveOptionButtons = new List<ArtifactTileButton>();

    // DC: Current list of moves being performed 
    protected List<SMove> activeMoves = new List<SMove>();
    //L: Queue of moves to perform on the grid from the artifact
    //L: IMPORTANT NOTE: The top element in the queue is always the current move being executed.
    protected Queue<SMove> moveQueue = new Queue<SMove>();
    public int maxMoveQueueSize = 3;    //L: Max size of the queue.

    private static UIArtifact _instance;
    
    public void Awake()
    {
        Init();
    }

    public void Init()
    {
        _instance = this;
    }

    public void Start()
    {
        SGridAnimator.OnSTileMoveEnd += QueueCheckAfterMove;

        OnButtonInteract += UpdatePushedDowns;
        SGridAnimator.OnSTileMoveEnd += UpdatePushedDowns;

        // Debug.Log(this is JungleArtifact);
    }

    public virtual void OnEnable()
    {

    }

    public virtual void OnDisable()
    {
        ClearQueues();
        //Debug.Log("Queue Cleared!");
    }

    public static UIArtifact GetInstance()
    {
        return _instance;
    }

    //L: Handles when the user attempts to drag and drop a button

    public virtual void ButtonDragged(BaseEventData eventData) { 
        // Debug.Log("draggi   ng");
        PointerEventData data = (PointerEventData) eventData;

        if (currentButton != null) 
        {
            // return;
            DeselectCurrentButton();
        }

        ArtifactTileButton dragged = data.pointerDrag.GetComponent<ArtifactTileButton>();
        if (!dragged.isTileActive)// || dragged.isForcedDown)
        {
            return;
        }
        else
        {
            SelectButton(dragged);
        }

        ArtifactTileButton hovered = null;
        if (data.pointerEnter != null && data.pointerEnter.name == "Image") 
        {
            hovered = data.pointerEnter.transform.parent.gameObject.GetComponent<ArtifactTileButton>();
        }

        
        foreach (ArtifactTileButton b in GetMoveOptions(dragged)) {
            if (b == hovered) 
            {
                b.SetHighlighted(false);
                b.buttonAnimator.sliderImage.sprite = b.hoverSprite; // = blankSprite
            }
            else 
            {
                b.SetHighlighted(true);
                b.ResetToIslandSprite();
            }
        }

        OnButtonInteract?.Invoke(this, null);
    }

    public virtual void ButtonDragEnd(BaseEventData eventData) {
        PointerEventData data = (PointerEventData) eventData;


        // Debug.Log("Sent drag end");
        // if (currentButton != null) 
        // {
        //     foreach (ArtifactTileButton b in GetMoveOptions(currentButton)) 
        //     {
        //         b.buttonAnimator.sliderImage.sprite = b.emptySprite;
        //     }
        //     return;
        // }

        ArtifactTileButton dragged = data.pointerDrag.GetComponent<ArtifactTileButton>();
        if (!dragged.isTileActive)// || dragged.isForcedDown)
        {
            return;
        }

        // reset move options visual
        List<ArtifactTileButton> moveOptions = GetMoveOptions(dragged);
        foreach (ArtifactTileButton b in moveOptions) {
            b.buttonAnimator.sliderImage.sprite = b.emptySprite;
        }
        
        ArtifactTileButton hovered = null;
        if (data.pointerEnter != null && data.pointerEnter.name == "Image") 
        {
            hovered = data.pointerEnter.transform.parent.gameObject.GetComponent<ArtifactTileButton>();
        }
        else 
        {
            // dragged should already be selected
            // SelectButton(dragged);
            return;
        }
        
        if (!hovered.isTileActive)
        {
            hovered.buttonAnimator.sliderImage.sprite = hovered.emptySprite;
        }
        //Debug.Log("dragged" + dragged.islandId + "hovered" + hovered.islandId);
        
        bool swapped = false;
        for (int i = 0; i < moveOptions.Count; i++) {
            ArtifactTileButton b = moveOptions[i];
            b.SetHighlighted(false);
            // b.buttonAnimator.sliderImage.sprite = b.emptySprite;
            if(b == hovered && !swapped) 
            {
                SelectButton(hovered);
                // CheckAndSwap(dragged, hovered);
                // SGridAnimator.OnSTileMoveEnd += dragged.AfterStileMoveDragged;
                swapped = true;
            }
        }
        if (!swapped) {
            SelectButton(dragged);
        }
        else
        {
            DeselectCurrentButton();
        }

        OnButtonInteract?.Invoke(this, null);
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

        OnButtonInteract?.Invoke(this, null);
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

                //L: Player makes a move while the tile is still moving, so add the button to the queue.
                CheckAndSwap(currentButton, button);

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

        OnButtonInteract?.Invoke(this, null);
    }

    // replaces adjacentButtons
    protected virtual List<ArtifactTileButton> GetMoveOptions(ArtifactTileButton button)
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
    protected virtual bool CheckAndSwap(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
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

    public void QueueCheckAndAdd(SMove move)
    {
        if (moveQueue.Count < maxMoveQueueSize)
        {
            moveQueue.Enqueue(move);
        } 
        else
        {
            Debug.LogWarning("Didn't add to the UIArtifact queue because it was full");
        }

    }

    protected virtual void QueueCheckAfterMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (e != null)
        {
            //Debug.Log("Checking for e");
            if (activeMoves.Contains(e.smove))
            {
                //Debug.Log("Move has been removed");
                activeMoves.Remove(e.smove);
            }
        }

        if (moveQueue.Count > 0)
        {
            //Debug.Log("Checking next queued move! Currently queue has " + moveQueue.Count + " moves...");

            SMove peekedMove = moveQueue.Peek();
            // check if the peekedMove interferes with any of current moves
            foreach (SMove m in activeMoves)
            {
                if (m.Overlaps(peekedMove))
                {
                    // Debug.Log("Move conflicts!");
                    return;
                }
            }

            // Debug.Log("Move doesn't conflict! Performing move.");

            // doesn't interfere! so do the move
            SGrid.current.Move(peekedMove);
            activeMoves.Add(moveQueue.Dequeue());
            QueueCheckAfterMove(this, null);
        }
    }

    public static void ClearQueues()
    {
        _instance.moveQueue.Clear();
    }

    public void UpdatePushedDowns(object sender, System.EventArgs e)
    {
       foreach (ArtifactTileButton b in _instance.buttons)
       {
           if (IsStileInActiveMoves(b.islandId))// || IsStileInQueue(b.islandId))
           {
               b.SetIsInMove(true);
           }
           else
           {
               b.SetIsInMove(false);
           }
       }
    }

    private bool IsStileInActiveMoves(int islandId)
    {
        foreach (SMove smove in activeMoves)
        {
            foreach (Movement m in smove.moves)
            {
                if (m.islandId == islandId)
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    private bool IsStileInQueue(int islandId)
    {
        foreach (SMove smove in moveQueue)
        {
            foreach (Movement m in smove.moves)
            {
                if (m.islandId == islandId)
                {
                    return true;
                }
            }
        }
        
        return false;
    }

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

    public static ArtifactTileButton GetButton(int x, int y)
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

    public ArtifactTileButton GetButton(int islandId) { // this causes issues with UITracker and setting prefab parents for some reason...

        foreach (ArtifactTileButton b in buttons)
        {
            if (b.islandId == islandId)
            {
                return b;
            }
        }

        return null;
    }
    

    // Returns a string like:   123_6##_4#5
    // for a grid like:  1 2 3
    //                   6 . .
    //        (0, 0) ->  4 . 5
    public static string GetGridString()
    {
        string s = "";
        for (int y = 3 - 1; y >= 0; y--)
        {
            for (int x = 0; x < 3; x++)
            {
                ArtifactTileButton b = GetButton(x, y);
                if (b.isTileActive)
                    s += b.islandId;
                else
                    s += "#";
            }
            if (y != 0)
            {
                s += "_";
            }
        }
        return s;
    }

    public static void AddButton(int islandId, bool shouldFlicker=true)
    {
        foreach (ArtifactTileButton b in _instance.buttons)
        {
            if (b.islandId == islandId)
            {
                b.SetTileActive(true);
                b.SetShouldFlicker(shouldFlicker);
                return;
            }
        }
    }

    public void FlickerNewTiles()
    {
        foreach (ArtifactTileButton b in buttons)
        {
            if (b.shouldFlicker)
            {
                b.Flicker(3);
            }
        }
    }

    public void FlickerAllOnce()
    {
        foreach (ArtifactTileButton b in buttons)
        {
            b.Flicker(1);
        }
    }
}
