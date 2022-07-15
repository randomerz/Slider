using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// ** THIS CLASS HAS BEEN UPDATED TO USE THE NEW SINGLETON BASE CLASS. PLEASE REPORT NEW ISSUES YOU SUSPECT ARE RELATED TO THIS CHANGE TO TRAVIS AND/OR DANIEL! **
public class UIArtifact : Singleton<UIArtifact>
{

    public ArtifactTileButton[] buttons;
    [SerializeField] protected int maxMoveQueueSize = 3;
    [SerializeField] private GameObject lightning;

    protected ArtifactTileButton buttonSelected;
    protected List<ArtifactTileButton> moveOptionButtons = new List<ArtifactTileButton>();
    protected List<SMove> activeMoves = new List<SMove>();    // DC: Current list of moves being performed 
    protected Queue<SMove> moveQueue = new Queue<SMove>();    //L: Queue of moves to perform on the grid from the artifact
    protected bool playerCanQueue;

    private bool didInit;

    public static System.EventHandler<System.EventArgs> OnButtonInteract;
    public static System.EventHandler<System.EventArgs> MoveMadeOnArtifact;


    
    protected void Awake()
    {
        if (!didInit)
        {
            Init();
        }
    }

    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {
        ClearQueues();
    }

    private void Start()
    {
        SGridAnimator.OnSTileMoveEnd += QueueCheckAfterMove;

        OnButtonInteract += UpdatePushedDowns;
        SGridAnimator.OnSTileMoveEnd += UpdatePushedDowns;
    }

    public void Init()
    {
        didInit = true;
        InitializeSingleton();

        EnableQueueing();
    }

    public void EnableQueueing()
    {
        playerCanQueue = true;
    }

    public void DisableQueueing()
    {
        playerCanQueue = false;
    }

    public static UIArtifact GetInstance()
    {
        return _instance;
    }

    //This is in case we have situations where the grid is modified without interacting with the artifact (Factory conveyors, Mountain anchor, MagiTech Desyncs)
    public void SetButtonPositionsToMatchGrid()
    {
        STile[,] grid = SGrid.Current.GetGrid();

        for (int x = 0; x < SGrid.Current.Width; x++)
        {
            for (int y = 0; y < SGrid.Current.Height; y++)
            {
                //If there is a tile at the position, set the corresponding button to that position, otherwise set an empty tile to that position
                if (grid[x, y] != null)
                {
                    foreach (ArtifactTileButton button in buttons)
                    {
                        if (button.islandId == grid[x, y].islandId)
                        {
                            button.SetPosition(x, y);
                        }
                    }
                }
            }
        }

        UpdateMoveOptions();
    }

    #region Drag and Drop
    public virtual void ButtonDragged(BaseEventData eventData)
    {
        PointerEventData data = (PointerEventData)eventData;

        DeselectSelectedButton();

        ArtifactTileButton dragged = data.pointerDrag.GetComponent<ArtifactTileButton>();
        if (!dragged.TileIsActive || dragged.MyStile.hasAnchor)// || dragged.isForcedDown)
        {
            return;
        }
        else
        {
            SelectButton(dragged, true);
        }

        ArtifactTileButton hovered = GetButtonHovered(data);

        SetButtonVisualsDuringMouseDrag(dragged, hovered);

        OnButtonInteract?.Invoke(this, null);
    }

    public virtual void ButtonDragEnd(BaseEventData eventData)
    {
        PointerEventData data = (PointerEventData)eventData;

        ArtifactTileButton dragged = data.pointerDrag.GetComponent<ArtifactTileButton>();
        if (!dragged.TileIsActive)// || dragged.isForcedDown)
        {
            return; //player didn't start drag on a button, don't do anything.
        }

        List<ArtifactTileButton> moveOptions = GetMoveOptions(dragged);
        ResetButtonsToEmptyIfInactive(moveOptions);

        ArtifactTileButton hovered = GetButtonHovered(data);
        if (hovered == null)
        {
            return; //player didn't release mouse on a button, don't do anything.
        }

        DoButtonDrag(dragged, hovered, moveOptions);
    }
    #endregion

    public virtual void SelectButton(ArtifactTileButton button, bool isDragged = false)
    {
        // Check if on movement cooldown
        //if (SGrid.GetStile(button.islandId).isMoving)

        if (buttonSelected != null)
        {
            if (moveOptionButtons.Contains(button))
            {
                TryDoMove(buttonSelected, button);
            } else 
            {
                bool sameButton = buttonSelected == button;
                DeselectSelectedButton();
                if (sameButton)
                {
                    return;
                }
            } 
        }

        if (buttonSelected == null) //This isn't an else if because we could have deselected the button and need to handle that case.
        {
            if (!button.TileIsActive)
            {
                return;
            }

            List<ArtifactTileButton> moveOptionButtons = GetMoveOptions(button);
            bool buttonWithLockedMovement = moveOptionButtons.Count == 0 || button.MyStile.hasAnchor;
            if (buttonWithLockedMovement)
            {
                return;
            }

            SetSelectedButton(button);

            bool autoMove = moveOptionButtons.Count == 1 && SettingsManager.AutoMove && !isDragged;
            if (autoMove)
            {
                TryDoMove(buttonSelected, moveOptionButtons[0]);
                DeselectSelectedButton();
            }
            else
            {
                UpdateMoveOptions();
            }
        }

        OnButtonInteract?.Invoke(this, null);
    }

    public void DeselectSelectedButton()
    {
        if (buttonSelected == null)
            return;

        buttonSelected.SetSelected(false);
        buttonSelected = null;

        ClearMoveOptions();

        //OnButtonInteract?.Invoke(this, null);
    }

    protected virtual List<ArtifactTileButton> GetMoveOptions(ArtifactTileButton button)
    {
        var options = new List<ArtifactTileButton>();

        Vector2Int[] dirs = {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down
        };

        foreach (Vector2Int dir in dirs)
        {
            ArtifactTileButton b = GetButton(button.x + dir.x, button.y + dir.y);
            int i = 2;
            while (b != null && !b.TileIsActive)
            {
                options.Add(b);
                b = GetButton(button.x + dir.x * i, button.y + dir.y * i);

                i++;
            }
        }

        return options;
    }

    protected void SwapButtons(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        int oldCurrX = buttonCurrent.x;
        int oldCurrY = buttonCurrent.y;
        buttonCurrent.SetPosition(buttonEmpty.x, buttonEmpty.y);
        buttonEmpty.SetPosition(oldCurrX, oldCurrY);
    }

    //L: Returns if the swap was successful.
    protected virtual bool TryDoMove(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        STile[,] currGrid = SGrid.Current.GetGrid();

        int x = buttonCurrent.x;
        int y = buttonCurrent.y;
        SMove swap = new SMoveSwap(x, y, buttonEmpty.x, buttonEmpty.y, buttonCurrent.islandId, buttonEmpty.islandId);
 
        // Debug.Log(SGrid.current.CanMove(swap) + " " + moveQueue.Count + " " + maxMoveQueueSize);
        // Debug.Log(buttonCurrent + " " + buttonEmpty);
        if (SGrid.Current.CanMove(swap) && moveQueue.Count < maxMoveQueueSize && playerCanQueue)
        {
            //L: Do the move
            MoveMadeOnArtifact?.Invoke(this, null);
            QueueCheckAndAdd(swap);
            SwapButtons(buttonCurrent, buttonEmpty);

            // Debug.Log("Added move to queue: current length " + moveQueue.Count);
            QueueCheckAfterMove(this, null);
            // if (moveQueue.Count == 1)
            // {
            //     SGrid.current.Move(moveQueue.Peek());
            // }
            UpdateMoveOptions();
            return true;
        }
        else
        {
            string debug = playerCanQueue ? "Player Queueing is disabled" : "Queue was full";
            Debug.Log($"Couldn't perform move! {debug}");
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

    //This is called every time a tile finishes moving
    public virtual void QueueCheckAfterMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        //If e is null, this is the first tile to move, if it's not null, then a previous tile moved.
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

            //Debug.Log("Move doesn't conflict! Performing move.");

            // doesn't interfere! so do the move
            SGrid.Current.Move(peekedMove);
            activeMoves.Add(moveQueue.Dequeue());
            QueueCheckAfterMove(this, null);
        }
    }

    public static bool ActiveMovesExist()
    {
        return _instance.activeMoves.Count > 0;
    }

    public static List<SMove> GetActiveMoves()
    {
        return _instance.activeMoves;
    }

    public static void ClearQueues()
    {
        _instance.moveQueue.Clear();
    }

    public bool FragRealignCheckAndSwap(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        STile[,] currGrid = SGrid.Current.GetGrid();

        int x = buttonCurrent.x;
        int y = buttonCurrent.y;
        SMove swap = new SMoveSwap(x, y, buttonEmpty.x, buttonEmpty.y, buttonCurrent.islandId, buttonEmpty.islandId);

        if (SGrid.Current.CanMove(swap) && moveQueue.Count < maxMoveQueueSize)
        {
            MoveMadeOnArtifact?.Invoke(this, null);
            QueueCheckAndAdd(swap);
            SwapButtons(buttonCurrent, buttonEmpty);
            QueueCheckAfterMove(this, null);
            return true;
        }
        else
        {
            string debug = playerCanQueue ? "Player Queueing is disabled" : "Queue was full";
            Debug.Log($"Couldn't perform move! {debug}");
            return false;
        }
    }

    public void UpdatePushedDowns(object sender, System.EventArgs e)
    {
       foreach (ArtifactTileButton b in _instance.buttons)
       {
            if(b.gameObject.activeSelf)
            {
                if (IsStileInActiveMoves(b.islandId))// || IsStileInQueue(b.islandId))
                {
                    //Debug.Log(b.islandId);
                    b.SetIsInMove(true);
                }
                else if(b.MyStile.hasAnchor)
                {
                    continue;
                }
                else
                {
                    b.SetIsInMove(false);
                }
            } 
       }
    }

    private bool IsStileInActiveMoves(int islandId)
    {
        foreach (SMove smove in activeMoves)
        {
            foreach (Movement m in smove.moves)
            {
                //Debug.Log(m.islandId);
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
    public static void SetLightningPos(ArtifactTileButton b)
    {
        //Debug.Log("Set Lightning Pos!");
        if (_instance.lightning == null) Debug.LogError("Lightning was not found! Set in inspector?");
        _instance.lightning.transform.SetParent(b.transform);
        _instance.lightning.transform.position = b.transform.position;
        _instance.lightning.gameObject.SetActive(true);
        b.SetLightning(true);
    }

    public static void SetLightningPos(int x, int y)
    {
        ArtifactTileButton b = GetButton(x, y);
        SetLightningPos(b);
    }

    public static void SetLightningPos(int islandId)
    {
        ArtifactTileButton b = _instance.GetButton(islandId);
        SetLightningPos(b);
    }

    public static void DisableLightning(bool disableHighlight)
    {
        if (!_instance.lightning.gameObject.activeInHierarchy)
        {
            return;
        }
        _instance.lightning.gameObject.SetActive(false);
        if (disableHighlight) _instance.lightning.transform.GetComponentInParent<ArtifactTileButton>().SetLightning(false);
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
        //Debug.LogWarning("Artifact tile button at " + x + ", " + y + " was not found!");
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
                if (b.TileIsActive)
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

    public virtual void AddButton(STile stile, bool shouldFlicker=true)
    {
        foreach (ArtifactTileButton b in buttons)
        {
            if (b.MyStile == stile)
            {
                b.UpdateTileActive();
                b.SetShouldFlicker(shouldFlicker);
                break;
            }
        }
    }

    public void FlickerNewTiles()
    {
        foreach (ArtifactTileButton b in buttons)
        {
            if (b.gameObject.activeSelf && b.shouldFlicker)
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

    public void MoveQueueEmpty(Condition c)
    {
        c.SetSpec(moveQueue.Count == 0 && activeMoves.Count == 0);
    }

    private void SetSelectedButton(ArtifactTileButton button)
    {
        buttonSelected = button;
        button.SetSelected(true);
    }

    private void UpdateMoveOptions()
    {
        if (buttonSelected != null)
        {
            moveOptionButtons = GetMoveOptions(buttonSelected);

            foreach (ArtifactTileButton b in buttons)
            {
                b.SetHighlighted(moveOptionButtons.Contains(b));
            }
        }
    }

    private void ClearMoveOptions()
    {
        foreach (ArtifactTileButton b in moveOptionButtons)
        {
            b.SetHighlighted(false);
        }
        moveOptionButtons.Clear();
    }

    private void ResetButtonsToEmptyIfInactive(List<ArtifactTileButton> buttons)
    {
        foreach (ArtifactTileButton b in buttons)
        {
            if (!b.TileIsActive)
            {
                b.SetSpriteToEmpty();
            }
        }
    }

    #region Drag and Drop

    private ArtifactTileButton GetButtonHovered(PointerEventData data)
    {
        ArtifactTileButton hovered = null;
        if (data.pointerEnter != null && data.pointerEnter.name == "Image")
        {
            hovered = data.pointerEnter.transform.parent.gameObject.GetComponent<ArtifactTileButton>();
        }
        return hovered;
    }

    private void SetButtonVisualsDuringMouseDrag(ArtifactTileButton dragged, ArtifactTileButton hovered)
    {
        foreach (ArtifactTileButton b in GetMoveOptions(dragged))
        {
            if (b == hovered)
            {
                b.SetHighlighted(false);
                b.SetSpriteToHover();
            }
            else
            {
                b.SetHighlighted(true);
                b.SetSpriteToIslandOrEmpty();
            }
        }
    }

    private void DoButtonDrag(ArtifactTileButton dragged, ArtifactTileButton hovered, List<ArtifactTileButton> moveOptions)
    {
        //Debug.Log($"dragged: {dragged.islandId} hovered: {hovered.islandId}");
        if (!hovered.TileIsActive)
        {
            hovered.SetSpriteToEmpty();
        }

        foreach (ArtifactTileButton b in moveOptions)
        {
            b.SetHighlighted(false);
            if (b == hovered)
            {
                SelectButton(hovered, true);
                DeselectSelectedButton();
                return;
            }
        }
        SelectButton(dragged, true);

        OnButtonInteract?.Invoke(this, null);
    }
}
#endregion