using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DesertArtifact : UIArtifact
{
    public Dictionary<(int, int), int> currGrid;
    private Queue<List<ATBPair>> storedSwaps;
    private List<ATBPair> latestSwaps;

    public static System.EventHandler<System.EventArgs> MirageDisappeared;

    private class ATBPair
    {
        public ArtifactTileButton current;
        public ArtifactTileButton furthest;
        public ATBPair(ArtifactTileButton c, ArtifactTileButton f)
        {
            current = c;
            furthest = f;
        }
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        currGrid = new Dictionary<(int, int), int>();
        storedSwaps = new Queue<List<ATBPair>>();
        latestSwaps = new List<ATBPair>();
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        SGridAnimator.OnSTileMoveEnd -= UpdatePushedDowns;
        OnButtonInteract -= UpdatePushedDowns;
    }

    public override void ButtonDragged(BaseEventData eventData) 
    { 
        if (IsDesertComplete())
        {
            return;
        }

        base.ButtonDragged(eventData);
    }

    public override void ButtonDragEnd(BaseEventData eventData) 
    {
        if (IsDesertComplete())
        {
            return;
        }
        
        base.ButtonDragEnd(eventData);
    }
    
    public override void SelectButton(ArtifactTileButton button, bool isDragged = false) 
    {
        // Being able to interact with the grid while in the temple looks weird and 
        // may lead to false assumptions when exploring it
        if (IsDesertComplete())
        {
            return;
        }
        
        base.SelectButton(button, isDragged);
    }

    private bool IsDesertComplete() => PlayerInventory.Contains("Slider 9", Area.Desert);

    //Chen: getMoveOptions will add buttons even if they're active for Desert sliding
    protected override List<ArtifactTileButton> GetMoveOptions(ArtifactTileButton button)
    {
        var options = new List<ArtifactTileButton>();
        if (button == null)
        {
            return options;
        }

        Vector2Int[] dirs = {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down
        };

        foreach (Vector2Int dir in dirs)
        {
            ArtifactTileButton b = GetButton(button.x + dir.x, button.y + dir.y);
            int i = 2;  //i=1 is checked in the above line, otherwise it will add the same option twice.
            while (b != null)
            {
                options.Add(b);
                b = GetButton(button.x + dir.x * i, button.y + dir.y * i);

                i++;
            }
        }

        return options;
    }

    public override bool TryQueueMoveFromButtonPair(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        UpdateMirageGrid();
        SMove move = ConstructMoveFromButtonPair(buttonCurrent, buttonEmpty);;
        buttonEmpty.SetSpriteToIslandOrEmpty();
        if (SGrid.Current.CanMove(move) && !QueueFull() && playerCanQueue)
        {
            if (move.moves.Count == 0)
            {
                latestSwaps.Clear();
                return false;
            }
            storedSwaps.Enqueue(latestSwaps);
            latestSwaps = new List<ATBPair>();
            QueueMoveFromButtonPair(move, buttonCurrent, buttonEmpty);
            return true;
        }
        else
        {
            latestSwaps.Clear();
            LogMoveFailure();
            return false;
        }
    }

    protected override void QueueMoveFromButtonPair(SMove move, ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        CoroutineUtils.ExecuteAfterEndOfFrame(() => {
            UIEffects.TakeScreenshot();
            QueueAdd(move);
            UpdateUI();
            AudioManager.Play("Slider Sand");
            ProcessQueue();
            UpdatePushedDowns(null, null);
            DeselectSelectedButton();
            MoveMadeOnArtifact?.Invoke(this, null);
        }, this);
    }

    protected override SMove ConstructMoveFromButtonPair(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        SSlideSwap move;
        int dx = buttonEmpty.x - buttonCurrent.x;
        int dy = buttonEmpty.y - buttonCurrent.y;
        
        if (dx > 0)
        {
            move = SlideRight();
        }
        else if (dx < 0)
        {
            move = SlideLeft();
        }
        else if (dy > 0)
        {
            move = SlideUp();
        }
        else
        {
            move = SlideDown();
        }

        return move;
    }
    
    public bool TryScrollScrapQueueMoveFromButtonPair(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        UpdateMirageGrid();
        SMove move = base.ConstructMoveFromButtonPair(buttonCurrent, buttonEmpty);
        if (SGrid.Current.CanMove(move) && !QueueFull() && playerCanQueue && playerCanAddSMoves)
        {
            if (move.moves.Count == 0)
            {
                return false;
            }
            ATBPair pair = new ATBPair(buttonCurrent,buttonEmpty);
            List<ATBPair> list = new List<ATBPair> {pair};
            storedSwaps.Enqueue(list);
            QueueMoveFromButtonPair(move, buttonCurrent, buttonEmpty);
            OnButtonInteract?.Invoke(this, null); //Since this is only called when you press a button technically
            return true;
        }
        else
        {
            LogMoveFailure();
            return false;
        }
    }

    public override void UpdatePushedDowns(object sender, EventArgs e)
    {
        foreach (ArtifactTileButton b in _instance.buttons)
        {
            if (b.gameObject.activeSelf)
            {
                if (IsStileInActiveMoves(b.islandId))// || IsStileInQueue(b.islandId))
                {
                    b.SetIsInMove(true);
                }
                else if (b.MyStile.hasAnchor)
                {
                    continue;
                }
                else
                {
                    b.SetIsInMove(false);
                    b.SetHighlighted(moveOptionButtons.Contains(b));
                }
            }
        }
    }

    private void UpdateUI()
    {
        if (storedSwaps.Count == 0)
        {
            Debug.LogWarning("Swaps queue was EMPTY");
            return;
        }
        List<ATBPair> pairs = storedSwaps.Dequeue();
        //Debug.Log($"# moves remaining: {storedSwaps.Count}");
        foreach (ATBPair s in pairs)
        {
            //Debug.Log($"current: {s.current} furthest: {s.furthest} \n target pos: {s.furthest.x}{s.furthest.y}");
            SwapButtons(s.current, s.furthest);
        }
    }

    private void UpdateMirageGrid()
    {
        Array.ForEach(buttons, plugin =>
        {
            currGrid[(plugin.x, plugin.y)] = plugin.islandId;
        });
    }
    #region SSlideSwap
    //Chen: Below are the 4 methods for sliding all tiles. UI swapping is handled here
    public SSlideSwap SlideRight()
    {
        List<Movement> swaps = new List<Movement>();
        
        for (int col = 0; col < 3; col++)
        {
            List<ArtifactTileButton> tiles = new List<ArtifactTileButton>
            {
            GetButton(1, col),
            GetButton(0, col)
            };

            UpdateSwaps(swaps, tiles, Vector2Int.right);
        }
        return new SSlideSwap(swaps);
    }

    public SSlideSwap SlideLeft()
    {
        List<Movement> swaps = new List<Movement>();

        for (int col = 0; col < 3; col++)
        {
            List<ArtifactTileButton> tiles = new List<ArtifactTileButton>
            {
            GetButton(1, col),
            GetButton(2, col)
            };
            UpdateSwaps(swaps, tiles, Vector2Int.left);
        }
        return new SSlideSwap(swaps);
    }

    public SSlideSwap SlideUp()
    {
        List<Movement> swaps = new List<Movement>();

        for (int row = 0; row < 3; row++)
        {
            List<ArtifactTileButton> tiles = new List<ArtifactTileButton>
            {
            GetButton(row, 1),
            GetButton(row, 0)
            };
            UpdateSwaps(swaps, tiles, Vector2Int.up);
        }
        return new SSlideSwap(swaps);
    }

    public SSlideSwap SlideDown()
    {
        List<Movement> swaps = new List<Movement>();

        for (int row = 0; row < 3; row++)
        {
            List<ArtifactTileButton> tiles = new List<ArtifactTileButton>
            {
            GetButton(row, 1),
            GetButton(row, 2),
            };
            UpdateSwaps(swaps, tiles, Vector2Int.down);
        }
        return new SSlideSwap(swaps);
    }

    private void UpdateSwaps(List<Movement> swaps, List<ArtifactTileButton> tiles, Vector2Int dir)
    {
        Vector2Int emptyButtonStart = new Vector2Int(-1, -1);
        Vector2Int emptyButtonEnd = new Vector2Int(-1, -1);
        int emptyIslandId = -1;
        foreach (ArtifactTileButton button in tiles)
        {
            if (button.TileIsActive)
            {
                (ArtifactTileButton, Vector2Int) furthest = GetLastEmpty(button, dir);
                if (furthest.Item1 != null)
                {
                    if (emptyButtonStart.x == -1)
                    {
                        emptyButtonStart = new Vector2Int(furthest.Item1.x, furthest.Item1.y);
                        emptyIslandId = furthest.Item1.islandId;
                    }
                    emptyButtonEnd = new Vector2Int(button.x, button.y);


                    swaps.Add(new Movement(button.x, button.y, furthest.Item2.x, furthest.Item2.y, button.islandId)); // bee dooo bee doo
                    latestSwaps.Add(new ATBPair(button, furthest.Item1));
                    //SwapButtons(button, furthest);  //L: This is kinda bad to do here since it's a side effect of constructing the move, but I don't want to break it (since I already did)
                }
            }
        }
        if (emptyButtonStart.x != -1 && emptyButtonEnd.x != -1)
        {
            swaps.Add(new Movement(emptyButtonStart.x, emptyButtonStart.y, emptyButtonEnd.x, emptyButtonEnd.y, emptyIslandId));
        }
    }

    //Chen: finds the furthest button a given button can go to given a direction
    private (ArtifactTileButton, Vector2Int) GetLastEmpty(ArtifactTileButton button, Vector2Int dir)
    {

        ArtifactTileButton curr = GetButton(button.x + dir.x, button.y + dir.y);
        ArtifactTileButton furthest = null;

        //Anchor Case: Don't move if the tile has anchor or if it's blocked by an anchor
        STile[,] grid = SGrid.Current.GetGrid();
        if (grid[button.x, button.y].hasAnchor || grid[curr.x, curr.y].hasAnchor)
        {
            return (null, Vector2Int.zero); // Code checks for null furthest
        }
        bool prevTileWasActive = false;
        Vector2Int cords = Vector2Int.zero;
        while (curr != null)
        {
            if (!curr.TileIsActive)
            {
                //Case 1: Tile slides to the empty space
                furthest = curr;
                cords.x = curr.x; 
                cords.y = curr.y;
                //Case 2: Two tiles slide one space. To prevent STile moving to same place
                if (prevTileWasActive)
                {
                    cords -= dir;
                }
            }
            prevTileWasActive = curr.TileIsActive;
            //L: Removed Case 2 because it is never used and doesn't return an empty button.


            curr = GetButton(curr.x + dir.x, curr.y + dir.y);
        }
        return (furthest, cords);
    }
    #endregion
}
