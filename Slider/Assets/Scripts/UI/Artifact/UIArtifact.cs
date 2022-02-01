using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class UIArtifact : MonoBehaviour
{
    // public Vector3 tempPosition = new Vector3(0,0,0);
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

    // public void OnDrawGizmos() {
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawSphere(tempPosition, 10);
    // }
    public void ButtonDragged(BaseEventData eventData) {
        Debug.Log("dragging");
        PointerEventData data = (PointerEventData) eventData;

        if (currentButton != null) 
        {
            return;
        }

        ArtifactTileButton dragged = data.pointerDrag.GetComponent<ArtifactTileButton>();
        if (!dragged.isTileActive)
        {
            return;
        }

        ArtifactTileButton hovered = null;
        if (data.pointerEnter != null && data.pointerEnter.name == "Image") 
        {
            hovered = data.pointerEnter.transform.parent.gameObject.GetComponent<ArtifactTileButton>();
        }

        
        foreach (ArtifactTileButton b in GetAdjacent(dragged)) {
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
        Debug.Log("Sent drag end");
        if (currentButton != null) 
        {
            return;
        }

        ArtifactTileButton dragged = data.pointerDrag.GetComponent<ArtifactTileButton>();
        if (!dragged.isTileActive)
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
        Debug.Log("dragged" + dragged.islandId + "hovered" + hovered.islandId);

        foreach (ArtifactTileButton b in GetAdjacent(dragged)) {
            b.buttonAnimator.sliderImage.sprite = b.emptySprite;
            if(b == hovered) 
            {
                Swap(dragged, hovered);

            }

        }
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
    
    public virtual void SelectButton(ArtifactTileButton button)
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
    protected List<ArtifactTileButton> GetAdjacent(ArtifactTileButton button)
    {
        adjacentButtons.Clear();

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
            int i = 1;
            bool didAdd = true;
            ArtifactTileButton b;
            while (didAdd && i < 99)
            {
                didAdd = false;

                b = GetButton(button.x + dir.x * i, button.y + dir.y * i);
                
                if (b != null && !b.isTileActive) {
                    adjacentButtons.Add(b);
                    didAdd = true;
                }
                    
                i += 1;
            }
        }

        return adjacentButtons;
    }

    private void Swap(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        STile[,] arr = SGrid.current.GetGrid();

        int x = buttonCurrent.x;
        int y = buttonCurrent.y;
        if (arr[x, y].linkTile == null) 
        {
            //Direction dir = DirectionUtil.V2D(new Vector2(buttonEmpty.x, buttonEmpty.y) - new Vector2(x, y));
            //EightPuzzle.MoveSlider(x, y, dir);

            SMove swap = new SMoveSwap(x, y, buttonEmpty.x, buttonEmpty.y);
            
            
            if (SGrid.current.CanMove(swap))
            {
                SGrid.current.Move(swap);
                buttonCurrent.SetPosition(buttonEmpty.x, buttonEmpty.y);
                StartCoroutine(SetForcePushedDown(buttonCurrent));
                buttonEmpty.SetPosition(x, y);
            }
            else
            {
                Debug.Log("Couldn't perform move!");
            }
        }
        else 
        {
            int dx = buttonEmpty.x - x;
            int dy = buttonEmpty.y - y;
            int linkx = -1;
            int linky = -1;
            for (int i=0; i<SGrid.current.width; i++) 
            {
                for (int j=0; j<SGrid.current.height; j++) 
                {
                    if (arr[x,y].linkTile == arr[i,j]) 
                    {
                        
                        linkx = i;
                        linky = j;
                    }
                }
            }
            SMove swap = new SMoveSwap(x, y, buttonEmpty.x, buttonEmpty.y);
            SMove swap2 = new SMoveSwap(linkx, linky, linkx+dx, linky+dy);
            Vector4Int movecoords = new Vector4Int(linkx, linky, linkx+dx, linky+dy);
            if (SGrid.current.CanMove(swap) && (OpenPath(movecoords, SGrid.current.GetGrid()) || arr[linkx+dx,linky+dy] == arr[x,y])) 
            {
                ArtifactTileButton buttonCurrent2 = null;
                ArtifactTileButton buttonEmpty2 = null;

                buttonCurrent2 = GetButton(linkx, linky);
                SGrid.current.Move(swap);
                SGrid.current.Move(swap2);
                buttonCurrent.SetPosition(buttonEmpty.x, buttonEmpty.y);
                StartCoroutine(SetForcePushedDown(buttonCurrent));
                buttonEmpty.SetPosition(x, y);
               
                buttonEmpty2 = GetButton(linkx+dx, linky+dy);
                StartCoroutine(SetForcePushedDown(buttonCurrent2));
                buttonCurrent2.SetPosition(linkx+dx, linky+dy);
                buttonEmpty2.SetPosition(linkx, linky);


            }
            else 
            {
                Debug.Log("illegal");
                AudioManager.Play("Artifact Error");
            }
        }


    }

    private bool OpenPath(Vector4Int move, STile[,] grid) {
        List<Vector2Int> checkedCoords = new List<Vector2Int>(); 
        int dx = move.z - move.x;
        int dy = move.w - move.y;
        Debug.Log(move.x+" "+move.y+" "+move.z+" "+move.w);
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
}
