using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MagitechArtifact : UIArtifact
{
    /*C: Note that this is on the *opposite* side of the grid from the anchor.
    *   IE if the anchor is dropped at (2,1), in the present, this vector will be (5, 1),
    *   the corresponding location in the past, since this is the location we need
    *   to compare against to check for move possibility
    */
    public Vector2Int desynchLocation = new Vector2Int(-1, -1);

    //C: likewise this is the ID of the *opposite* Stile
    public int desynchIslandId = -1; 

    public UnityEvent onDesynchStart;
    public UnityEvent onDesynchEnd;

    public bool isInPast = false;

    public Image background;
    public Sprite presentBackgroundSprite;
    public Sprite pastBackgroundSprite;

    public override void OnEnable()
    {
        base.OnEnable();
        Anchor.OnAnchorInteract += OnAnchorInteract;
        SetButtonsAndBackground();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        Anchor.OnAnchorInteract -= OnAnchorInteract;
    }

    private void Update()
    {
        if(isInPast && Player.GetInstance().transform.position.x < 34 || !isInPast && Player.GetInstance().transform.position.x > 34)
        {
            isInPast = !isInPast;
            SetButtonsAndBackground();
        }
    }

    private void OnAnchorInteract(object sender, Anchor.OnAnchorInteractArgs interactArgs)
    {
        if (interactArgs.drop)
        {
            STile dropTile = interactArgs.stile;
            if(dropTile!= null)
            {
                desynchLocation = new Vector2Int(FindAlt(dropTile.x,3), dropTile.y);
                desynchIslandId = FindAlt(dropTile.islandId, 9);
                onDesynchStart.Invoke();
            }
        }
        else
        {
            onDesynchEnd.Invoke();
            RestoreOnEndDesynch();
            desynchLocation = new Vector2Int(-1, -1);
            desynchIslandId = -1;
        }
    }


    /* C: moves the tiles on the non-anchored timeline to align with the tiles on the 
     * anchored timeline. We need to both restore the tile that was desynched and
     * the non-active tiles, since they might no longer be aligned
     */
    private void RestoreOnEndDesynch()
    {
        STile[,] temp = SGrid.current.GetGrid();
        int[,] currGrid = new int[6,3];
        int[,] newGrid = new int[6,3];
        for(int x = 0; x < 6; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                currGrid[x,y] = temp[x,y].islandId;
                newGrid[x,y] = temp[x,y].islandId;
            }
        }
        
        int offset = (desynchLocation.x / 3) * 3 ;
        for(int x = 0; x < 3; x++)
        {
            for(int y = 0; y < 3; y++)
            {
                newGrid[x + offset, y] = FindAlt(currGrid[x - offset + 3,y], 9); 
            }
        }

        /* C: you can uncomment this if desynching isn't working, it might help locate the source of the problem
        string output = "";
        
        for(int y = 2; y >=0 ; y--)
        {
            for (int x = 0; x < 6; x++)
            {
                output+=currGrid[x,y];
                output+= "\t";
            }
            output+="\n";
        }
        output+="\n";
        output+="\n";
        for(int y = 2; y >=0 ; y--)
        {
            for (int x = 0; x < 6; x++)
            {
                output+=newGrid[x,y];
                output+= "\t";
            }
            output+="\n";
        }
        Debug.Log(output);
        */

        SGrid.current.SetGrid(newGrid);
    }

    protected override List<ArtifactTileButton> GetMoveOptions(ArtifactTileButton button)
    {
        moveOptionButtons.Clear();

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
            while (b != null && !b.isTileActive && (b.x/3 == button.x/3) ) //C: check that x/3 is the same to not count moves on the other side of the grid as valid
            {
                if(CheckDesynch(button, b)) //C: check for anchor on opposite tile
                    moveOptionButtons.Add(b);
                b = GetButton(button.x + dir.x * i, button.y + dir.y * i);
                i++;
            }
        }

        return moveOptionButtons;
    }

    private bool CheckDesynch(ArtifactTileButton b1, ArtifactTileButton b2)
    {
        if(desynchLocation.x != -1)
        {
            return (b1.islandId == desynchIslandId || !(b2.x == desynchLocation.x && b2.y == desynchLocation.y));
        }
        return true; //C: No desynch active, so valid move
    }

    //C: basically just modulus. Used to find corresponding values on either side of the grid
    private int FindAlt(int num, int offset)
    {
        return (num + offset) % (offset * 2);
    }


    protected override bool CheckAndSwap(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        STile[,] currGrid = SGrid.current.GetGrid();

        int x = buttonCurrent.x;
        int y = buttonCurrent.y;
        
        SMove swap;
        if(buttonCurrent.islandId == desynchIslandId)
        {
            swap = new SMoveSwap(x, y, buttonEmpty.x, buttonEmpty.y, buttonCurrent.islandId, buttonEmpty.islandId);
        }
        else
        {
            swap = new SMoveSyncedMove(x, y, buttonEmpty.x, buttonEmpty.y, buttonCurrent.islandId, buttonEmpty.islandId);
        }
      
        if (SGrid.current.CanMove(swap) && moveQueue.Count < maxMoveQueueSize && PlayerCanQueue)
        {
            MoveMadeOnArtifact?.Invoke(this, null);
            QueueCheckAndAdd(swap);
            SwapButtons(buttonCurrent, buttonEmpty);
            QueueCheckAfterMove(this, null);
            return true;
        }
        else
        {
            string debug = PlayerCanQueue ? "Player Queueing is disabled" : "Queue was full";
            Debug.Log($"Couldn't perform move! {debug}");
            return false;
        }
    }

    public void SetButtonsAndBackground()
    {
        foreach (ArtifactTileButton b in buttons)
        {
            if (b.islandId > 9 && isInPast || b.islandId <= 9 && !isInPast)
            {
                b.gameObject.SetActive(true);
                STile myStile = SGrid.current.GetStile(b.islandId);
                b.SetTileActive(myStile.isTileActive);
                b.SetPosition(myStile.x, myStile.y);
            }
            else
                b.gameObject.SetActive(false);
        }
        background.sprite = isInPast? pastBackgroundSprite : presentBackgroundSprite;
    }
}
