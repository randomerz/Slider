using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OceanArtifact : UIArtifact
{
    public UIRotateParams[] rotateParams; // Bot Left, BR, TL, TR
    public OceanArtifactHighlights oceanHighlights;

    private bool canRotate = true;

    public new void Awake()
    {
        base.Awake();
    }

    public override void OnEnable()
    {
        base.OnEnable();

        OnButtonInteract += UpdateHighlights;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        OnButtonInteract -= UpdateHighlights;
    }

    public override void ButtonDragged(BaseEventData eventData) 
    { 
        // do nothing
    }

    public override void ButtonDragEnd(BaseEventData eventData) 
    {
        // do nothing
    }
    
    public override void SelectButton(ArtifactTileButton button) 
    {
        // do nothing
    }
    
    // equivalent as CheckAndSwap in UIArtifact.cs but it doesn't remove
    public void RotateTiles(int x, int y, bool rotateCCW)
    {
        if (!canRotate)
            return;

        // logic for finding which tiles to rotate
        List<Vector2Int> SMoveRotateArr = new List<Vector2Int> { 
                new Vector2Int(x, y),
                new Vector2Int(x, y + 1),
                new Vector2Int(x + 1, y + 1),
                new Vector2Int(x + 1, y),
            };
        List<int> islandIds = new List<int>();
        List<Vector2Int> anchoredPositions = new List<Vector2Int>();

        List<ArtifactTileButton> tb = new List<ArtifactTileButton>{
            GetButton(x, y),
            GetButton(x, y + 1),
            GetButton(x + 1, y + 1),
            GetButton(x + 1, y)
        };

        if (rotateCCW) 
        {
            SMoveRotateArr.Reverse();
            tb.Reverse();
        }

        bool isAtLeastOneActive = false;
        for (int i=3; i>=0; i--)
        {
            int curX = SMoveRotateArr[i].x;
            int curY = SMoveRotateArr[i].y;

            STile[,] grid = SGrid.current.GetGrid();

            if (grid[curX, curY].isTileActive)
            {
                if (grid[curX, curY].hasAnchor)
                {
                    SMoveRotateArr.RemoveAt(i);
                    tb.RemoveAt(i);
                    anchoredPositions.Add(new Vector2Int(curX, curY));
                    continue;
                }
                else
                {
                    isAtLeastOneActive = true;
                }
            }
            islandIds.Add(grid[curX, curY].islandId);
        }

        if (!isAtLeastOneActive)
        {
            return;
        }

        // performing the rotate smove
        // todo: if can rotate
        // if (SGrid.current.CanRotate)
        if (moveQueue.Count < maxMoveQueueSize)
        {
            SMoveRotate rotate = new SMoveRotate(SMoveRotateArr, islandIds, rotateCCW);
            rotate.anchoredPositions = anchoredPositions;
            QueueCheckAndAdd(rotate);
            // SwapButtons(buttonCurrent, buttonEmpty);
            // update UI button positions
            for (int i = 0; i < tb.Count; i++)
            {
                tb[i].SetPosition(SMoveRotateArr[(i + 1) % tb.Count].x, SMoveRotateArr[(i + 1) % tb.Count].y);
            }

            // SGrid.current.Move(rotate);
            QueueCheckAfterMove(this, null);
            
        }
        else 
        {
            Debug.Log("Couldn't perform move! (queue full?)");
        }

        OnButtonInteract?.Invoke(this, null);
    }

    // DC: this plays the animation when the tiles actually move... should we keep track of UI similarly?
    public override void QueueCheckAfterMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (e != null)
        {
            if (activeMoves.Contains(e.smove))
            {
                activeMoves.Remove(e.smove);
            }
        }

        if (moveQueue.Count > 0)
        {
            SMoveRotate peekedMove = moveQueue.Peek() as SMoveRotate;
            // check if the peekedMove interferes with any of current moves
            foreach (SMove m in activeMoves)
            {
                if (m.Overlaps(peekedMove))
                {
                    // Debug.Log("Move conflicts!");
                    return;
                }
            }

            int minX = peekedMove.moves[0].startLoc.x;
            int minY = peekedMove.moves[0].startLoc.y;

            foreach (Movement v in peekedMove.moves)
            {
                minX = Mathf.Min(v.startLoc.x, minX);
                minY = Mathf.Min(v.startLoc.y, minY);
            }
            
            rotateParams[minY * 2 + minX].RotateArrow(peekedMove.isCCW);
        }

        base.QueueCheckAfterMove(sender, e);
    }

    public void UpdateHighlights(object sender, System.EventArgs e)
    {
        string gridString = GetGridString();

        oceanHighlights.SetBoat(CheckGrid.contains(gridString, "41"));
        oceanHighlights.SetVolcanoEast(CheckGrid.contains(gridString, "95"));
        oceanHighlights.SetVolcanoNorth(CheckGrid.contains(gridString, "4...9"));
        oceanHighlights.SetVolcanoWest(CheckGrid.contains(gridString, "89"));
        oceanHighlights.SetVolcanoSouth(CheckGrid.contains(gridString, "9...3"));
    }

    public void SetCanRotate(bool value)
    {
        canRotate = value;
        if (!value)
        {
            moveQueue.Clear();
            activeMoves.Clear();
        }
    }
}