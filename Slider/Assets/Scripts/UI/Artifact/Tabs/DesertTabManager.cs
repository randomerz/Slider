using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DesertTabManager : ArtifactTabManager
{
    [Header("Frag Realign Tab")]
    [SerializeField] private ArtifactTab fragRealignTab;
    [SerializeField] private Animator rearrangingFragTabAnimator;
    [SerializeField] private Image rearrangingFragTabImage;
    private ArtifactTileButton fragSwapTile;
    private ArtifactTileButton middle;

    private Coroutine fragSwapTileCycleRoutine;

    private bool successfullyUsedOnceBefore = false;
    [SerializeField] private PlayerActionHints playerActionHints;

    [SerializeField] private Sprite[] tabSprites; //0 = top left, 1 = top middle, 2 = top right, 3 = left middle, 4 = right middle, 5 = bottom left, 6 = bottom middle, 7 = bottom right
    /*
    [SerializeField] private GameObject grayMiddleTab;
    private Image grayMiddleTabImage;
    private Vector2 grayMiddleTabHoverPos;
    private Vector2 grayMiddleTabOGPos;*/
    public override void SetCurrentScreen(int screenIndex)
    {
        if(realignTab == null)
            InitTabs();
        if (PlayerInventory.Contains("Scroll of Realigning", Area.Desert))
        {
            if (SGrid.Current.GetActiveTiles().Count == SGrid.Current.GetTotalNumTiles() //Realigning case
                && SGrid.GetNumButtonCompletions() != SGrid.Current.GetTotalNumTiles())
            {
                realignTab.SetIsVisible(screenIndex == realignTab.homeScreen);
                saveTab.SetIsVisible(false);
                loadTab.SetIsVisible(false);
                fragRealignTab.SetIsVisible(false);
            }
            else //Save Load Case
            {
                realignTab.SetIsVisible(false);
                saveTab.SetIsVisible(screenIndex == saveTab.homeScreen);
                loadTab.SetIsVisible(screenIndex == loadTab.homeScreen);
                SetSaveLoadTabSprites(SGrid.Current.HasRealigningGrid());
                fragRealignTab.SetIsVisible(false);
            }
        }
        else if (PlayerInventory.Contains("Scroll Frag", Area.Desert) && SGrid.Current.GetActiveTiles().Count != SGrid.Current.GetTotalNumTiles())
        {
            fragRealignTab.SetIsVisible(screenIndex == fragRealignTab.homeScreen);
        }
        else
        {
            realignTab.SetIsVisible(false);
            saveTab.SetIsVisible(false);
            loadTab.SetIsVisible(false);
            fragRealignTab.SetIsVisible(false);
            //This is cursed but I have no idea what else to do for the moment
            UIArtifact.DisableLightning(true);
            middle?.SetLightning(false);
            fragSwapTile?.SetLightning(false);
        }
    }

    private void Start()
    {
        /*
        grayMiddleTabImage = grayMiddleTab.GetComponent<Image>();
        grayMiddleTabOGPos = grayMiddleTab.transform.position;
        grayMiddleTabHoverPos = new Vector2(grayMiddleTabOGPos.x + 1, grayMiddleTabOGPos.y);*/
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        SGridAnimator.OnSTileMoveEndLateLate += OnMoveEnd;
    }

    private void OnMoveEnd(object sender, System.EventArgs e)
    {
        /*
        middle = UIArtifact.GetButton(1, 1);

        if (middle.TileIsActive && !grayMiddleTabImage.enabled)
        {
            grayMiddleTabImage.enabled = true;
        }
        else if (!middle.TileIsActive && grayMiddleTabImage.enabled)
        {
            grayMiddleTabImage.enabled = false;
        }*/
    }

    public void FragRearrangeOnClick()
    {
        // Do the rearranging!
        if (middle == fragSwapTile || !middle.TileIsActive)
        {
            AudioManager.Play("Artifact Error");
            return;
        }
        DesertArtifact artifact = (DesertArtifact)uiArtifactMenus.uiArtifact;
        artifact.TryFragQueueMoveFromButtonPair(middle, fragSwapTile);
        artifact.UpdatePushedDowns(null, null);
        artifact.DeselectSelectedButton();

        if (!successfullyUsedOnceBefore)
        {
            playerActionHints.DisableHint("scrollscrap");
            successfullyUsedOnceBefore = true;
        }

        FragRearrangeOnHoverExit();
    }

    public void FragRearrangeOnHoverEnter()
    {
        rearrangingFragTabAnimator.enabled = false;
        //grayMiddleTab.transform.position = grayMiddleTabHoverPos;

        //Preview!
        middle = UIArtifact.GetButton(1, 1);
        
        if (middle.TileIsActive)
        {
            UpdateFragSwapTile();
            UIArtifact.SetLightningPos(1, 1);
            middle.SetScrollHighlight(true);

            SetFragSwapTile(fragSwapTile);
        }
    }

    public void FragRearrangeOnHoverExit()
    {
        if (fragSwapTileCycleRoutine != null)
        {
            StopCoroutine(fragSwapTileCycleRoutine);
        }

        rearrangingFragTabAnimator.enabled = true;
        //grayMiddleTab.transform.position = grayMiddleTabOGPos;

        //Reset preview
        UIArtifact.DisableLightning(true);
        middle.SetScrollHighlight(false);
        if (fragSwapTile != null)
        {
            fragSwapTile.SetScrollHighlight(false);
        }
    }

    private void UpdateFragSwapTile()
    {
        if (fragSwapTileCycleRoutine != null)
        {
            StopCoroutine(fragSwapTileCycleRoutine);
        }

        ArtifactTileButton tile8 = uiArtifactMenus.uiArtifact.GetButton(8);
        //do we have a better way to check if there's more than one empty tile?
        if (!tile8.TileIsActive)
        {
            fragSwapTile = tile8;
            fragSwapTileCycleRoutine = StartCoroutine(CycleFragSwapTileBetweenEmptyTiles());
        }
        else
        {
            fragSwapTile = uiArtifactMenus.uiArtifact.GetButton(9);
            SetFragSwapTile(fragSwapTile);
        }
    }

    private IEnumerator CycleFragSwapTileBetweenEmptyTiles()
    {
        ArtifactTileButton[] emptyTiles = { uiArtifactMenus.uiArtifact.GetButton(8), uiArtifactMenus.uiArtifact.GetButton(9) };
        
        while (true)
        {
            foreach(ArtifactTileButton emptyTile in emptyTiles)
            {
                fragSwapTile = emptyTile;
                SetFragSwapTile(fragSwapTile);
                yield return new WaitForSeconds(1);
                fragSwapTile.SetScrollHighlight(false);
            }
        }
    }

    private void SetFragSwapTile(ArtifactTileButton tile)
    {
        tile.SetScrollHighlight(true);

        switch (tile.x)
        {
            case (0):
                switch (tile.y)
                {
                    case (0):
                        rearrangingFragTabImage.sprite = tabSprites[5];
                        break;
                    case (1):
                        rearrangingFragTabImage.sprite = tabSprites[3];
                        break;
                    case (2):
                        rearrangingFragTabImage.sprite = tabSprites[0];
                        break;
                }
                break;
            case (1):
                switch (tile.y)
                {
                    case (0):
                        rearrangingFragTabImage.sprite = tabSprites[6];
                        break;
                    case (2):
                        rearrangingFragTabImage.sprite = tabSprites[1];
                        break;
                }
                break;
            case (2):
                switch (tile.y)
                {
                    case (0):
                        rearrangingFragTabImage.sprite = tabSprites[7];
                        break;
                    case (1):
                        rearrangingFragTabImage.sprite = tabSprites[4];
                        break;
                    case (2):
                        rearrangingFragTabImage.sprite = tabSprites[2];
                        break;
                }
                break;
        }
    }
}
