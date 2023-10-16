using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MagiTechArtifact : UIArtifact
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
    private bool isPreview = false;
    private bool isDesynchSoundPlaying = false;

    private AudioManager.ManagedInstance desyncTearLoopSound;

    public Image background;
    public Sprite presentBackgroundSprite;
    public Sprite pastBackgroundSprite;


    protected override void OnEnable()
    {
        base.OnEnable();
        Anchor.OnAnchorInteract += OnAnchorInteract;
        SetButtonsAndBackground(isInPast);
    }


    protected override void OnDisable()
    {
        base.OnDisable();
        Anchor.OnAnchorInteract -= OnAnchorInteract;
    }

    protected override void Update()
    {
        base.Update();

        if (isInPast != PlayerIsInPast)
        {
            isInPast = PlayerIsInPast;
            SetButtonsAndBackground(isInPast);
        }
        //Lightning stuff
        if (desynchIslandId != -1)
        {
            ArtifactTileButton desyncedButton = GetButton(desynchIslandId);
            UpdateButtonPositions();
            if (desynchLocation.x != desyncedButton.x || desynchLocation.y != desyncedButton.y)
            {
                if (!isDesynchSoundPlaying)
                {
                    isDesynchSoundPlaying = true;
                    desyncTearLoopSound = AudioManager.PickSound("Desync Tear Open").AndPlay();
                }
                ArtifactTileButton pastButton = desynchIslandId <= 9 ? GetButton(FindAltId(desynchIslandId)) : GetButton(desynchIslandId);
                if (isInPast != isPreview) SetLightningPos(pastButton);
                else SetLightningPos(GetButton(FindAltId(pastButton.islandId)));
            }
            else
            {
                if (isDesynchSoundPlaying)
                {
                    isDesynchSoundPlaying = false;
                    desyncTearLoopSound.Stop();
                    AudioManager.PickSound("Desync Tear Close").AndPlay();
                }
                DisableLightning(false);
            }
        }
        else DisableLightning(true);
    }

    private void OnAnchorInteract(object sender, Anchor.OnAnchorInteractArgs interactArgs)
    {
        STile dropTile = interactArgs.stile;
        if (dropTile != null)
        {
            if (interactArgs.drop)
            {
                desynchLocation = FindAltCoords(dropTile.x, dropTile.y);
                desynchIslandId = FindAltId(dropTile.islandId);
                GetButton(desynchIslandId).SetLightning(true);
                GetButton(dropTile.islandId).SetLightning(true);
                onDesynchStart.Invoke();
            }
            else if (desynchIslandId != -1) //L: Might break smth, but techincally desync only ends if it began in the first place.
            {
                onDesynchEnd.Invoke();
                RestoreOnEndDesynch();
                desynchLocation = new Vector2Int(-1, -1);
                desynchIslandId = -1;
            }
            GetButton(dropTile.islandId).buttonAnimator.SetAnchored(interactArgs.drop);
        }
    }


    /* C: moves the tiles on the non-anchored timeline to align with the tiles on the 
     * anchored timeline. We need to both restore the tile that was desynched and
     * the non-active tiles, since they might no longer be aligned
     */
    private void RestoreOnEndDesynch()
    {
        STile[,] temp = SGrid.Current.GetGrid();
        int[,] currGrid = new int[6, 3];
        int[,] newGrid = new int[6, 3];
        for (int x = 0; x < 6; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                currGrid[x, y] = temp[x, y].islandId;
                newGrid[x, y] = temp[x, y].islandId;
            }
        }

        int offset = (desynchLocation.x / 3) * 3;
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                newGrid[x + offset, y] = FindAltId(currGrid[x - offset + 3, y]);
            }
        }
        DisableLightning(true);
        GetButton(desynchIslandId).SetLightning(false);
        GetButton(FindAltId(desynchIslandId)).SetLightning(false);
        UpdatePushedDowns(null, null);
        // C: you can uncomment this if desynching isn't working, it might help locate the source of the problem
        // PrintGrid(currGrid, newGrid);

        SGrid.Current.SetGrid(newGrid);
    }

    //C: debugging tool if desynch gets broken
    public void PrintGrid(int[,] currGrid, int[,] newGrid)
    {
        string output = "";

        for (int y = 2; y >= 0; y--)
        {
            for (int x = 0; x < 6; x++)
            {
                output += currGrid[x, y];
                output += "\t";
            }
            output += "\n";
        }
        output += "\n";
        output += "\n";
        for (int y = 2; y >= 0; y--)
        {
            for (int x = 0; x < 6; x++)
            {
                output += newGrid[x, y];
                output += "\t";
            }
            output += "\n";
        }
        Debug.Log(output);
    }

    protected override List<ArtifactTileButton> GetMoveOptions(ArtifactTileButton button)
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
            while (b != null && !b.TileIsActive && !CheckDesynch(button, b)
            && button.x / 3 == b.x / 3)
            {
                options.Add(b);
                b = GetButton(button.x + dir.x * i, button.y + dir.y * i);
                i++;
            }
        }
        return options;
    }

    //L: Negating this so that it returns true when there is a desync.
    private bool CheckDesynch(ArtifactTileButton selected, ArtifactTileButton empty)
    {
        if (desynchLocation.x != -1)
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
        return (islandId == 9) ? 18 : (islandId + 9) % 18;
    }

    protected override SMove ConstructMoveFromButtonPair(ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        SMove move;
        if (buttonCurrent.islandId == desynchIslandId)
        {
            move = base.ConstructMoveFromButtonPair(buttonCurrent, buttonEmpty);
        }
        else
        {
            move = new SMoveSyncedMove(buttonCurrent.x, buttonCurrent.y, buttonEmpty.x, buttonEmpty.y, buttonCurrent.islandId, buttonEmpty.islandId);
        }
        return move;
    }

    protected override void QueueMoveFromButtonPair(SMove move, ArtifactTileButton buttonCurrent, ArtifactTileButton buttonEmpty)
    {
        ArtifactTileButton currAlt = GetButton(FindAltId(buttonCurrent.islandId));
        ArtifactTileButton emptyAlt = GetButton(FindAltId(buttonEmpty.islandId));
        
        SwapButtons(currAlt, emptyAlt, false);

        base.QueueMoveFromButtonPair(move, buttonCurrent, buttonEmpty);
    }

    public override void AddButton(STile stile, bool shouldFlicker = true)
    {
        base.AddButton(stile, shouldFlicker);
        SetButtonsAndBackground(isInPast);
    }

    public void SetButtonsAndBackground(bool past)
    {
        foreach (ArtifactTileButton b in buttons)
        {
            if (b.islandId > 9 && past || b.islandId <= 9 && !past)
            {
                b.gameObject.SetActive(true);             
            }
            else
                b.gameObject.SetActive(false);
        }
        background.sprite = past ? pastBackgroundSprite : presentBackgroundSprite;
    }

    public void SetPreview(bool enable)
    {
        SetButtonsAndBackground(isInPast != enable);
        isPreview = enable;
    }

    private void UpdateButtonPositions()
    {
        foreach (ArtifactTileButton b in buttons)
        {
            if (b.islandId > 9 && !isInPast || b.islandId <= 9 && isInPast)
            {
                b.UpdateTileActive();
            }
        }
    }

    public override void UpdatePushedDowns(object sender, System.EventArgs e)
    {
        foreach (ArtifactTileButton b in _instance.buttons)
        {
            if (IsStileInActiveMoves(b.islandId))// || IsStileInQueue(b.islandId))
            {
                //Debug.Log(b.islandId);
                b.SetIsInMove(true);
            }
            else if (b.MyStile.hasAnchor)
            {
                continue;
            }
            else
            {
                b.SetIsInMove(false);
            }
        }
    }


    // for ui icons

    public bool IsDisplayingPast()
    {
        // xor
        return (isInPast && !isPreview) || (!isInPast && isPreview);
    }
}
