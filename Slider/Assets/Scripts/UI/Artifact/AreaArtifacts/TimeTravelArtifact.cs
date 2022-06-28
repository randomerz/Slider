using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Serialization;

//L: Changed name bc I plan to use the same system for Factory stuff.
public class TimeTravelArtifact : UIArtifact
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

    public bool PlayerIsInPast
    {
        get
        {
            //67 is roughly between the end of the present map and the beginning of the past map, so it should be a safe value.
            return Player.GetInstance().transform.position.x > 67;
        }
    }
    private bool isInPast = false;

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
        if(isInPast != PlayerIsInPast)
        {
            isInPast = PlayerIsInPast;
            SetButtonsAndBackground();
        }
    }

    private void OnAnchorInteract(object sender, Anchor.OnAnchorInteractArgs interactArgs)
    {
        if (interactArgs.drop)
        {
            STile dropTile = interactArgs.stile;
            if(dropTile != null)
            {
                desynchLocation = FindAltCoords(dropTile.x, dropTile.y);
                desynchIslandId = FindAltId(dropTile.islandId);
                onDesynchStart.Invoke();
            }
        }
        else if (desynchIslandId != -1) //L: Might break smth, but techincally desync only ends if it began in the first place.
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
        
        int offset = (desynchLocation.x / 3) * 3;
        for(int x = 0; x < 3; x++)
        {
            for(int y = 0; y < 3; y++)
            {
                newGrid[x + offset, y] = FindAltId(currGrid[x - offset + 3,y]); 
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
        List<ArtifactTileButton> options = base.GetMoveOptions(button);

        for (int i = 0; i < options.Count; ) {
            ArtifactTileButton option = options[i];

            bool differentTimePeriod = button.x / 3 != option.x / 3; //C: check that x/3 is the same to not count moves on the other side of the grid as valid
            if (differentTimePeriod || CheckDesynch(button, option))    //C: Check anchor on opposite tile.
            {
                options.RemoveAt(i);
            } else
            {
                i++;
            }
        }

        return options;
    }

    //L: Negating this so that it returns true when there is a desync.
    private bool CheckDesynch(ArtifactTileButton selected, ArtifactTileButton empty)
    {
        if(desynchLocation.x != -1)
        {
            //If we're trying to 
            return !(selected.islandId == desynchIslandId) && (empty.x == desynchLocation.x && empty.y == desynchLocation.y);
        }
        return false; //C: No desynch active, so valid move
    }

    //C: basically just modulus. Used to find corresponding values on either side of the grid
    private Vector2Int FindAltCoords(int x, int y)
    {
        return new Vector2Int((x + 3) % 6, y);
    }

    private int FindAltId(int islandId)
    {
        return (islandId + 9) % 18;
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

    public override void AddButton(STile stile, bool shouldFlicker = true)
    {
        base.AddButton(stile, shouldFlicker);
        SetButtonsAndBackground();
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
