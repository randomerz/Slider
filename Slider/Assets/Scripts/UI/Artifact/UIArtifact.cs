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
    private ArtifactTileButton currentButton;
    //L: The available buttons the player has to move to from currentButton
    private List<ArtifactTileButton> moveOptionButtons = new List<ArtifactTileButton>();
    //queue is used for when the player makes multiple moves before a move has finished.
    private Queue<SMove> queue;
    public int maxMovesBuffered = 2;
    IEnumerator queueEmptyCoroutine;

    private static UIArtifact _instance;
    
    public void Awake()
    {
        _instance = this;
        queue = new Queue<SMove>();
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
        queue = new Queue<SMove>();
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
                    if ((queue.Count < maxMovesBuffered) && CheckAndSwap(currentButton, button, true))
                    {
                        QueueCheckAndAdd(new SMoveSwap(currentButton.x, currentButton.y, button.x, button.y));
                    }

                    //Debug.Log(currentButton.gameObject.name + " added to the queue!");
                    //Debug.Log(button.gameObject.name + " added to the Queue");
                } else
                {
                    //L: Player makes a move immediately
                    CheckAndSwap(currentButton, button);
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
    private void SwapButtons(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        int oldCurrX = buttonCurrent.x;
        int oldCurrY = buttonCurrent.y;
        buttonCurrent.SetPosition(buttonEmpty.x, buttonEmpty.y);
        buttonEmpty.SetPosition(oldCurrX, oldCurrY);
    }

    //L: updateGrid - if this is false, it will just update the UI without actually moving the tiles.
    private bool CheckAndSwap(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty, bool queuedMove = false)
    {
        STile[,] currGrid = SGrid.current.GetGrid();

        int x = buttonCurrent.x;
        int y = buttonCurrent.y;
        if (currGrid[x, y].linkTile == null) 
        {
            //Direction dir = DirectionUtil.V2D(new Vector2(buttonEmpty.x, buttonEmpty.y) - new Vector2(x, y));
            //EightPuzzle.MoveSlider(x, y, dir);

            SMove swap = new SMoveSwap(x, y, buttonEmpty.x, buttonEmpty.y);
            
            if (SGrid.current.CanMove(swap))
            {
                //L: If the move is queued (queuedMove) then we need to wait until the move is dequed before doing it to the grid.
                if (!queuedMove)
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
        else 
        {
            //L: Below is to handle the case for if you have linked tiles.
            int dx = buttonEmpty.x - x;
            int dy = buttonEmpty.y - y;

            Vector2Int linkCoords = GetLinkTileCoords(buttonCurrent);
            int linkx = linkCoords.x;
            int linky = linkCoords.y;
            
            SMove swap = new SMoveSwap(x, y, buttonEmpty.x, buttonEmpty.y);
            SMove swap2 = new SMoveSwap(linkx, linky, linkx+dx, linky+dy);
            Vector4Int movecoords = new Vector4Int(linkx, linky, linkx+dx, linky+dy);
            if (SGrid.current.CanMove(swap) && (OpenPath(movecoords, SGrid.current.GetGrid()) || currGrid[linkx+dx,linky+dy] == currGrid[x,y])) 
            {
                if (!queuedMove)
                {
                    SGrid.current.Move(swap);
                    SGrid.current.Move(swap2);
                    StartCoroutine(WaitForMoveThenEmptyQueue(buttonCurrent));
                }

                //L: Swap the current button and the link button
                SwapButtons(buttonCurrent, buttonEmpty);
                SwapButtons(GetButton(linkx, linky), GetButton(linkx + dx, linky + dy));

                return true;
            }
            else 
            {
                // Debug.Log("illegal");
                AudioManager.Play("Artifact Error");
                return false;
            }
        }
    }

    private void UpdateGridAfterButtonSwap(int prevX, int prevY, int x, int y)
    {
        SMove swap = new SMoveSwap(prevX, prevY, x, y);
        SGrid.current.Move(swap);

        if (SGrid.current.GetGrid()[prevX, prevY].linkTile != null)
        {
            Vector2Int linkCoords = GetLinkTileCoords(currentButton);
            int linkx = linkCoords.x;
            int linky = linkCoords.y;
            int dx = currentButton.x - prevX;
            int dy = currentButton.y - prevY;
            SMove swap2 = new SMoveSwap(linkx, linky, linkx + dx, linky + dy);
            SGrid.current.Move(swap2);
        }
    }

    private Vector2Int GetLinkTileCoords(ArtifactTileButton buttonCurrent)
    {
        STile[,] currGrid = SGrid.current.GetGrid();
        int x = buttonCurrent.x;
        int y = buttonCurrent.y;

        //L: These aren't actually needed
        //I'm just testing if getting the tile coords gives different results from the double for loop,
        //and if it does, we can delete the loop entirely.
        int oldLinkx = currGrid[x, y].linkTile.x;
        int oldLinky = currGrid[x, y].linkTile.y;

        int linkx = oldLinkx;
        int linky = oldLinky;
        for (int i = 0; i < SGrid.current.width; i++)
        {
            for (int j = 0; j < SGrid.current.height; j++)
            {
                if (currGrid[x, y].linkTile == currGrid[i, j])
                {

                    linkx = i;
                    linky = j;
                }
            }
        }
        if (oldLinkx != linkx || oldLinky != linky)
        {
            Debug.LogError("We need dumb for loop for links :(");
        }

        return new Vector2Int(x, y);
    }

    //Checks if the move can happen on the grid.
    private bool OpenPath(Vector4Int move, STile[,] grid) {
        List<Vector2Int> checkedCoords = new List<Vector2Int>(); 
        int dx = move.z - move.x;
        int dy = move.w - move.y;
        // Debug.Log(move.x+" "+move.y+" "+move.z+" "+move.w);
        int toCheck = Math.Max(Math.Abs(dx), Math.Abs(dy));
        if (dx == 0) {
            int dir = dy / Math.Abs(dy);
            for (int i=1; i <= toCheck; i++) {
                if (grid[move.x, move.y+i*dir].isTileActive) {
                    return false;
                }  
            }
        }
        else if (dy == 0) {
            int dir = dx / Math.Abs(dx);
            for (int i=1; i <= toCheck; i++) {
                if (grid[move.x+i*dir, move.y].isTileActive) {
                    return false;
                }  
            }
        }
        return true;
    }

    public void QueueCheckAndAdd(SMove move)
    {
        if (queue.Count < maxMovesBuffered)
        {
            queue.Enqueue(move);
        } else
        {
            Debug.LogWarning("Didn't add to the UIArtifact queue because it was full");
        }

    }

    public bool QueueCheckAndRemove()
    {
        if (queue.Count != 0)
        {
            SMove move = queue.Dequeue();
            //Debug.Log("Swapping " + currentButton.gameObject.name + " with " + emptyButton.gameObject.name);

            //L: Update the grid since the Artifact UI should have already updated.
            Vector4Int swap = ((SMoveSwap) move).GetSwapAsVector();
            UpdateGridAfterButtonSwap(swap.x, swap.y, swap.z, swap.w);
            return true;
        }

        return false;
    }

    private IEnumerator WaitForMoveThenEmptyQueue(ArtifactTileButton button)
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
