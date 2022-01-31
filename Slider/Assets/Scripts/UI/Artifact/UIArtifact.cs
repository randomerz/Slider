using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIArtifact : MonoBehaviour
{
    public ArtifactTileButton[] buttons;
    private ArtifactTileButton currentButton;
    private List<ArtifactTileButton> adjacentButtons = new List<ArtifactTileButton>();
    private Queue<ArtifactTileButton> Queue;

    private static UIArtifact _instance;
    
    public void Awake()
    {
        _instance = this;
        Queue = new Queue<ArtifactTileButton>();
    }

    public static UIArtifact GetInstance()
    {
        return _instance;
    }

    public void OnDisable()
    {
        Queue = new Queue<ArtifactTileButton>();
        Debug.Log("Queue Cleared!");
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
        if (currentButton != null && currentButton.isForcedDown && adjacentButtons.Contains(button))
        {
            //Debug.Log(currentButton.gameObject.name + " added to the queue!");
            //Debug.Log(button.gameObject.name + " added to the Queue");
            QueueAdd(currentButton, button);
            DeselectCurrentButton();
        }

        else if (currentButton == button)
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
        int x = buttonCurrent.x;
        int y = buttonCurrent.y;
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

    private IEnumerator SetForcePushedDown(ArtifactTileButton button)
    {
        button.SetForcedPushedDown(true);

        yield return new WaitForSeconds(1);
        
        button.SetForcedPushedDown(false);
        CheckQueue();
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

    public void QueueAdd(ArtifactTileButton currentButton, ArtifactTileButton buttonEmpty)
    {
        Queue.Enqueue(currentButton);
        Queue.Enqueue(buttonEmpty);
    }

    public void CheckQueue()
    {
        if (Queue.Count != 0)
        {
            ArtifactTileButton currentButton = Queue.Dequeue();
            ArtifactTileButton emptyButton = Queue.Dequeue();
            //Debug.Log("Swapping " + currentButton.gameObject.name + " with " + emptyButton.gameObject.name);
            Swap(currentButton, emptyButton);
        }
    }
}
