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
    public static Vector2Int desyncLocation = new Vector2Int(-1, -1);

    //C: likewise this is the ID of the *opposite* Stile
    public int desyncIslandId = -1;

    public UnityEvent onDesyncStart;
    public UnityEvent onDesyncEnd;

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
    private bool isDesyncSoundPlaying = false;

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
        if (desyncIslandId != -1)
        {
            ArtifactTileButton desyncedButton = GetButton(desyncIslandId);
            UpdateButtonPositions();
            if (desyncLocation.x != desyncedButton.x || desyncLocation.y != desyncedButton.y)
            {
                if (!isDesyncSoundPlaying)
                {
                    isDesyncSoundPlaying = true;
                    desyncTearLoopSound = AudioManager.PickSound("Desync Tear Open").AndPlay();
                }
                ArtifactTileButton pastButton = desyncIslandId <= 9 ? GetButton(FindAltId(desyncIslandId)) : GetButton(desyncIslandId);
                if (isInPast != isPreview) SetLightningPos(pastButton);
                else SetLightningPos(GetButton(FindAltId(pastButton.islandId)));
            }
            else
            {
                if (isDesyncSoundPlaying)
                {
                    isDesyncSoundPlaying = false;
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
                desyncLocation = FindAltCoords(dropTile.x, dropTile.y);
                desyncIslandId = FindAltId(dropTile.islandId);
                GetButton(desyncIslandId).SetLightning(true);
                GetButton(dropTile.islandId).SetLightning(true);
                onDesyncStart.Invoke();
            }
            else if (desyncIslandId != -1)
            {
                if (isDesyncSoundPlaying)
                {
                    isDesyncSoundPlaying = false;
                    desyncTearLoopSound.Stop();
                    AudioManager.PickSound("Desync Tear Close").AndPlay();
                }
                onDesyncEnd.Invoke();
                RestoreOnEndDesync();
                desyncLocation = new Vector2Int(-1, -1);
                desyncIslandId = -1;
            }
            GetButton(dropTile.islandId).buttonAnimator.SetAnchored(interactArgs.drop);
        }
    }

    private void RestoreOnEndDesync()
    {

        DisableLightning(true);
        GetButton(desyncIslandId).SetLightning(false);
        GetButton(FindAltId(desyncIslandId)).SetLightning(false);
        UpdatePushedDowns(null, null);
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
            while (b != null && !b.TileIsActive && !DoesDesyncBlockMove(button, b)
            && button.x / 3 == b.x / 3)
            {
                options.Add(b);
                b = GetButton(button.x + dir.x * i, button.y + dir.y * i);
                i++;
            }
        }
        return options;
    }

    //Checks if the "empty" space should actually be blocked by the desync tile
    private bool DoesDesyncBlockMove(ArtifactTileButton selected, ArtifactTileButton empty)
    {
        if (desyncLocation.x != -1)
        {
            bool selectedIsDesync = selected.islandId == desyncIslandId;
            bool emptyIsDesync = empty.x == desyncLocation.x && empty.y == desyncLocation.y;
            return !selectedIsDesync && emptyIsDesync;
        }
        return false; //C: No desync active, so valid move
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
        if (buttonCurrent.islandId == desyncIslandId)
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

        //If Not a desync, swap both pairs of buttons
        if(move is SMoveSyncedMove)
        {
            SwapButtons(currAlt, emptyAlt, false);
            base.QueueMoveFromButtonPair(move, buttonCurrent, buttonEmpty);
        }
        //Else, find the correct button pair and swap them
        else
        {
            int idInMove = move.moves[0].islandId;
            if(currAlt.islandId == idInMove || emptyAlt.islandId == idInMove)
            {
                base.QueueMoveFromButtonPair(move, currAlt, emptyAlt);
            }
            else
            {
                base.QueueMoveFromButtonPair(move, buttonCurrent, buttonEmpty);
            }
        }
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
