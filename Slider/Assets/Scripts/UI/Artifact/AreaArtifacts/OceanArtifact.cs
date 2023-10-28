using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OceanArtifact : UIArtifact
{
    public UIRotateParams[] rotateParams; // Bot Left, BR, TL, TR
    public Transform[] waveSlideSoundOrigins; // Bot Left, BR, TL, TR
    public OceanArtifactHighlights oceanHighlights;

    private bool canRotate = true;

    [SerializeField] private OceanControllerSupportButtonsHolder oceanControllerSupportButtonsHolder;
    [SerializeField] private Button topLeftControllerButton;

    private new void Awake()
    {
        base.Awake();

        Player.OnControlSchemeChanged += OnPlayerControlSchemeChanged;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        OnButtonInteract += UpdateHighlights;
        UIArtifactMenus.OnArtifactOpened += UpdateHighlights;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        OnButtonInteract -= UpdateHighlights;
        UIArtifactMenus.OnArtifactOpened -= UpdateHighlights;

        Player.OnControlSchemeChanged -= OnPlayerControlSchemeChanged;
    }

    public override void ButtonDragged(BaseEventData eventData) 
    { 
        // do nothing
    }

    public override void ButtonDragEnd(BaseEventData eventData) 
    {
        // do nothing
    }
    
    public override void SelectButton(ArtifactTileButton button, bool isDragged = false) 
    {
        // do nothing
    }

    /// <summary>
    /// Routine that rotates all tiles in the ocean grid (for Fezziwig)
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    public IEnumerator RotateAllTiles(System.Action callback = null)
    {
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                RotateTiles(x, y, false, false);
                
                //Waits for active moves to be clear to call rotate tiles again
                //so the artifact buttons don't all change at once
                yield return new WaitUntil(() => activeMoves.Count == 0);
            }
        }
        callback?.Invoke();
    }
    
    // equivalent as CheckAndSwap in UIArtifact.cs but it doesn't remove
    public void RotateTiles(int x, int y, bool rotateCCW, bool checkCanRotate = true)
    {
        if (checkCanRotate && !canRotate)
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

            STile[,] grid = SGrid.Current.GetGrid();

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
            QueueAdd(rotate);
            // SwapButtons(buttonCurrent, buttonEmpty);
            // update UI button positions
            for (int i = 0; i < tb.Count; i++)
            {
                tb[i].SetPosition(SMoveRotateArr[(i + 1) % tb.Count].x, SMoveRotateArr[(i + 1) % tb.Count].y, true);
            }

            AudioManager.Play("Slide Ocean", waveSlideSoundOrigins[x + y * 2]);
            // SGrid.current.Move(rotate);
            ProcessQueue();
            
        }
        else 
        {
            LogMoveFailure();
        }
        OnButtonInteract?.Invoke(this, null);
    }

    // DC: this plays the animation when the tiles actually move... should we keep track of UI similarly?
    public override void ProcessQueue()
    {
        if (moveQueue.Count > 0)
        {
            SMoveRotate peekedMove = moveQueue.Peek() as SMoveRotate;
            // check if the peekedMove interferes with any of current moves
            if (MoveOverlapsWithActiveMove(peekedMove))
            {
                return;
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

        if (oceanControllerSupportButtonsHolder != null && oceanControllerSupportButtonsHolder.isActiveAndEnabled /*Player.GetInstance().GetCurrentControlScheme() == "Controller"*/)
        {
            oceanControllerSupportButtonsHolder.MakeLastControllerButtonClickedDisappear();
        }

        base.ProcessQueue();
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

    /// <summary>
    /// Sets the value of can rotate and clears queue if false
    /// </summary>
    /// <param name="value"></param>
    public void SetCanRotate(bool value)
    {
        canRotate = value;
        if (!value)
        {
            moveQueue.Clear();
            activeMoves.Clear();
        }
    }

    /// <summary>
    /// Sets can rotate without clearing the queue
    /// </summary>
    /// <param name="value"></param>
    public void AllowRotate(bool value)
    {
        canRotate = value;
    }    
    private void OnPlayerControlSchemeChanged(string newControlScheme)
    {
        if (newControlScheme == "Controller")
        {
            oceanControllerSupportButtonsHolder.gameObject.SetActive(true);
            topLeftControllerButton.Select();
        }
        else
        {
            oceanControllerSupportButtonsHolder.gameObject.SetActive(false);
        }
    }
}