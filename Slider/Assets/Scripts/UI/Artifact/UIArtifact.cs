using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIArtifact : MonoBehaviour
{
    public ArtifactTileButton[] buttons;
    private ArtifactTileButton currentButton;
    private List<ArtifactTileButton> adjacentButtons = new List<ArtifactTileButton>();

    private static UIArtifact _instance;
    
    public void Awake()
    {
        _instance = this;
    }

    public static UIArtifact GetInstance()
    {
        return _instance;
    }

    public void DeselectCurrentButton()
    {
        if (currentButton == null)
            return;

        currentButton.SetPushedDown(false);
        foreach (ArtifactTileButton b in adjacentButtons)
        {
            b.SetHighlighted(false);
        }
        currentButton = null;
        adjacentButtons.Clear();
    }
    
    public void SelectButton(ArtifactTileButton button)
    {
        // Check if on movement cooldown
        //if (SGrid.GetStile(button.islandId).isMoving)
        if (button.isForcedDown)
        {
            //Debug.Log("on cooldown!");
            return;
        }

        if (currentButton == button)
        {
            DeselectCurrentButton();
        }
        else if (adjacentButtons.Contains(button)) // compare by id?
        {
            Swap(currentButton, button);
            DeselectCurrentButton();
        }
        else
        {
            DeselectCurrentButton();

            if (!button.isTileActive)
            {
                return;
            }

            adjacentButtons = GetAdjacent(button);
            if (adjacentButtons.Count == 0)
            {
                return;
            }
            else
            {
                //Debug.Log("Selected button " + button.islandId);
                currentButton = button;
                button.SetPushedDown(true);
                foreach (ArtifactTileButton b in adjacentButtons)
                {
                    b.SetHighlighted(true);
                }
            }
        }
        //else
        //{
        //    // default deselect
        //    DeselectCurrentButton();
        //}
    }

    // replaces adjacentButtons
    private List<ArtifactTileButton> GetAdjacent(ArtifactTileButton button)
    {
        adjacentButtons.Clear();

        //Vector2 buttPos = new Vector2(button.x, button.y);
        foreach (ArtifactTileButton b in buttons)
        {
            //if (!b.isTileActive && (buttPos - new Vector2(b.x, b.y)).magnitude == 1)
            if (!b.isTileActive && (button.x == b.x || button.y == b.y))
            {
                adjacentButtons.Add(b);
            }
        }

        return adjacentButtons;
    }

    private void Swap(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        int x = buttonCurrent.x;
        int y = buttonCurrent.y;
        //Direction dir = DirectionUtil.V2D(new Vector2(buttonEmpty.x, buttonEmpty.y) - new Vector2(x, y));
        //EightPuzzle.MoveSlider(x, y, dir);

        SMove swap = new SMoveSwap(x, y, buttonEmpty.x, buttonEmpty.y);
        if (SGrid.current.CanMove(swap))
        {
            SGrid.current.Move(swap);
        }
        else
        {
            Debug.Log("Couldn't perform move!");
        }

        buttonCurrent.SetPosition(buttonEmpty.x, buttonEmpty.y);
        StartCoroutine(SetForcePushedDown(buttonCurrent));
        buttonEmpty.SetPosition(x, y);
    }

    private IEnumerator SetForcePushedDown(ArtifactTileButton button)
    {
        button.SetForcedPushedDown(true);

        yield return new WaitForSeconds(1);
        
        button.SetForcedPushedDown(false);
    }

    //public static void UpdatePushedDowns()
    //{
    //    foreach (ArtifactButton b in _instance.buttons)
    //    {
    //        b.UpdatePushedDown();
    //    }
    //}
    
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

    private ArtifactTileButton GetButton(int x, int y)
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




    // temporary
    public void RotateTiles(int x, int y)
    {
        List<Vector2Int> SMoveRotateArr = new List<Vector2Int> { 
                new Vector2Int(x, y),
                new Vector2Int(x, y + 1),
                new Vector2Int(x + 1, y + 1),
                new Vector2Int(x + 1, y),
            };

        for (int i=3; i>=0; i--)
        {
            int curX = SMoveRotateArr[i].x;
            int curY = SMoveRotateArr[i].y;

            STile[,] grid = SGrid.current.GetGrid();

            if (grid[curX, curY].hasAnchor)
            {
                SMoveRotateArr.RemoveAt(i);
            }
        }

        SMove rotate = new SMoveRotate(SMoveRotateArr);
        // todo: if can rotate
        SGrid.current.Move(rotate);

        ArtifactTileButton[] tb =
        {
            GetButton(x, y),
            GetButton(x, y + 1),
            GetButton(x + 1, y + 1),
            GetButton(x + 1, y)
        };

        tb[0].SetPosition(x, y + 1);
        tb[1].SetPosition(x + 1, y + 1);
        tb[2].SetPosition(x + 1, y);
        tb[3].SetPosition(x, y);
    }
}
