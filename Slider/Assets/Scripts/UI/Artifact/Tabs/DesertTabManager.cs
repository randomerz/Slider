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

    [SerializeField] private GameObject grayMiddleTab;

    private Sprite[,] tabSpritesArray;


    private void Start()
    {
        BuildTabSpritesArray();
    }

    private void BuildTabSpritesArray()
    {
        tabSpritesArray = new Sprite[3,3];
        for(int i = 0; i < 8; i ++)
        {
            int loc = i < 4 ? i : i+1;
            int y = (8 - loc) / 3;
            int x = loc % 3;
            tabSpritesArray[x,y] = tabSprites[i];
        }
    }


    public override void SetCurrentScreen(int screenIndex)
    {
        if(realignTab == null)
            InitTabs();
        if (PlayerInventory.Contains("Scroll of Realigning", Area.Desert))
        {
            if (IsInRealignMode())
            {
                EnableRealignTab(screenIndex);
            }
            else 
            {
                EnableSaveLoadTab(screenIndex);
            }
        }
        else if (PlayerInventory.Contains("Scroll Frag", Area.Desert) && SGrid.Current.GetActiveTiles().Count != SGrid.Current.GetTotalNumTiles())
        {
            fragRealignTab.SetIsVisible(screenIndex == fragRealignTab.homeScreen);
        }
        else
        {
           DisableTabs();
        }
    }

    private bool IsInRealignMode()
    {
        return SGrid.Current.GetActiveTiles().Count == SGrid.Current.GetTotalNumTiles()
                && SGrid.GetNumButtonCompletions() != SGrid.Current.GetTotalNumTiles();
    }

    private void EnableRealignTab(int screenIndex)
    {
        realignTab.SetIsVisible(screenIndex == realignTab.homeScreen);
        saveTab.SetIsVisible(false);
        loadTab.SetIsVisible(false);
        fragRealignTab.SetIsVisible(false);
    }

    private void EnableSaveLoadTab(int screenIndex)
    {
        realignTab.SetIsVisible(false);
        saveTab.SetIsVisible(screenIndex == saveTab.homeScreen);
        loadTab.SetIsVisible(screenIndex == loadTab.homeScreen);
        SetSaveLoadTabSprites(SGrid.Current.HasRealigningGrid());
        fragRealignTab.SetIsVisible(false);
    }

    private void DisableTabs()
    {
        realignTab.SetIsVisible(false);
        saveTab.SetIsVisible(false);
        loadTab.SetIsVisible(false);
        fragRealignTab.SetIsVisible(false);
        UIArtifact.DisableLightning(true);
        middle?.SetLightning(false);
        fragSwapTile?.SetLightning(false);
    }

    private void Update()
    {
        UpdateGrayTab();
    }

    private void UpdateGrayTab()
    {
        middle = UIArtifact.GetButton(1, 1);
        if(middle != null && middle.TileIsActive) 
        {
            grayMiddleTab.SetActive(false);
        }
        else
        {
            grayMiddleTab.SetActive(true);
        }
    }

    public void FragRearrangeOnClick()
    {
        middle = UIArtifact.GetButton(1, 1);
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

        //Preview!
        middle = UIArtifact.GetButton(1, 1);
        
        if (middle.TileIsActive)
        {
            UpdateFragSwapTile();
            UIArtifact.SetLightningPos(1, 1);
            middle.SetScrollHighlight(true);

            SetFragIconToShowSwapTile(fragSwapTile);
        }
    }

    public void FragRearrangeOnHoverExit()
    {
        middle = UIArtifact.GetButton(1, 1);
        if (fragSwapTileCycleRoutine != null)
        {
            StopCoroutine(fragSwapTileCycleRoutine);
        }

        rearrangingFragTabAnimator.enabled = true;

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
            SetFragIconToShowSwapTile(fragSwapTile);
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
                SetFragIconToShowSwapTile(fragSwapTile);
                yield return new WaitForSeconds(0.5f);
                fragSwapTile.SetScrollHighlight(false);
            }
        }
    }

    private void SetFragIconToShowSwapTile(ArtifactTileButton tile)
    {
        tile.SetScrollHighlight(true);
        rearrangingFragTabImage.sprite = tabSpritesArray[tile.x, tile.y];
    }
}
