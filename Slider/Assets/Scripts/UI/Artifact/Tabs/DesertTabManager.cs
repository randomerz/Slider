using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DesertTabManager : ArtifactTabManager
{
    [Header("Scroll Scrap Realign Tab")]
    [SerializeField] private ArtifactTab scrollScrapRealignTab;
    [SerializeField] private Animator scrollScrapTabAnimator;
    [SerializeField] private Image scrollScrapTabImage;
    private ArtifactTileButton scrollScrapSwapTile;
    private ArtifactTileButton middle;

    private Coroutine scrollScrapSwapTileCycleCoroutine;

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
        if (realignTab == null)
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
        else if (PlayerInventory.Contains("Scroll Scrap", Area.Desert) && SGrid.Current.GetActiveTiles().Count != SGrid.Current.GetTotalNumTiles())
        {
            scrollScrapRealignTab.SetIsVisible(screenIndex == scrollScrapRealignTab.homeScreen);
        }
        else
        {
           DisableTabs();
        }

        // For debug so the hint isnt always there
        if (PlayerInventory.Contains("Scroll Scrap", Area.Desert) && PlayerInventory.Contains("Slider 8", Area.Desert))
        {
            playerActionHints.DisableHint("scrollscrap");
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
        scrollScrapRealignTab.SetIsVisible(false);
    }

    private void EnableSaveLoadTab(int screenIndex)
    {
        realignTab.SetIsVisible(false);
        saveTab.SetIsVisible(screenIndex == saveTab.homeScreen);
        loadTab.SetIsVisible(screenIndex == loadTab.homeScreen);
        SetSaveLoadTabSprites(SGrid.Current.HasRealigningGrid());
        scrollScrapRealignTab.SetIsVisible(false);
    }

    private void DisableTabs()
    {
        realignTab.SetIsVisible(false);
        saveTab.SetIsVisible(false);
        loadTab.SetIsVisible(false);
        scrollScrapRealignTab.SetIsVisible(false);
        UIArtifact.DisableLightning(true);
        middle?.SetLightning(false, styleIndex: 2);
        scrollScrapSwapTile?.SetLightning(false, styleIndex: 2);
    }

    private void Update()
    {
        UpdateGrayTab();
    }

    private void UpdateGrayTab()
    {
        middle = UIArtifact.GetButton(1, 1);
        if (middle != null && middle.TileIsActive) 
        {
            grayMiddleTab.SetActive(false);
        }
        else
        {
            grayMiddleTab.SetActive(true);
        }
    }

    public void ScrollScrapRearrangeOnClick()
    {
        middle = UIArtifact.GetButton(1, 1);
        if (middle == scrollScrapSwapTile || !middle.TileIsActive)
        {
            AudioManager.Play("Artifact Error");
            return;
        }
        DesertArtifact artifact = (DesertArtifact)uiArtifactMenus.uiArtifact;
        artifact.TryScrollScrapQueueMoveFromButtonPair(middle, scrollScrapSwapTile);
        // artifact.UpdatePushedDowns(null, null);
        artifact.DeselectSelectedButton();

        if (!successfullyUsedOnceBefore)
        {
            playerActionHints.DisableHint("scrollscrap");
            successfullyUsedOnceBefore = true;
        }

        ScrollScrapRearrangeOnHoverExit();
    }

    public void ScrollScrapRearrangeOnHoverEnter()
    {
        scrollScrapTabAnimator.enabled = false;

        //Preview!
        middle = UIArtifact.GetButton(1, 1);
        
        if (middle.TileIsActive)
        {
            UpdateScrollScrapSwapTile();
            UIArtifact.SetLightningPos(1, 1, styleIndex: 2);
            // middle.SetLightning(true, styleIndex: 2);

            SetScrollScrapIconToShowSwapTile(scrollScrapSwapTile);
        }
    }

    public void ScrollScrapRearrangeOnHoverExit()
    {
        middle = UIArtifact.GetButton(1, 1);
        if (scrollScrapSwapTileCycleCoroutine != null)
        {
            StopCoroutine(scrollScrapSwapTileCycleCoroutine);
        }

        scrollScrapTabAnimator.enabled = true;

        //Reset preview
        UIArtifact.DisableLightning(true);
        // middle.SetLightning(false, styleIndex: 2);
        if (scrollScrapSwapTile != null)
        {
            scrollScrapSwapTile.SetLightning(false, styleIndex: 2);
        }
    }

    private void UpdateScrollScrapSwapTile()
    {
        if (scrollScrapSwapTileCycleCoroutine != null)
        {
            StopCoroutine(scrollScrapSwapTileCycleCoroutine);
        }

        ArtifactTileButton tile8 = uiArtifactMenus.uiArtifact.GetButton(8);
        //do we have a better way to check if there's more than one empty tile?
        if (!tile8.TileIsActive)
        {
            scrollScrapSwapTile = tile8;
            scrollScrapSwapTileCycleCoroutine = StartCoroutine(CycleScrollScrapSwapTileBetweenEmptyTiles());
        }
        else
        {
            scrollScrapSwapTile = uiArtifactMenus.uiArtifact.GetButton(9);
            SetScrollScrapIconToShowSwapTile(scrollScrapSwapTile);
        }
    }

    private IEnumerator CycleScrollScrapSwapTileBetweenEmptyTiles()
    {
        ArtifactTileButton[] emptyTiles = { uiArtifactMenus.uiArtifact.GetButton(8), uiArtifactMenus.uiArtifact.GetButton(9) };
        
        while (true)
        {
            foreach(ArtifactTileButton emptyTile in emptyTiles)
            {
                scrollScrapSwapTile = emptyTile;
                SetScrollScrapIconToShowSwapTile(scrollScrapSwapTile);
                yield return new WaitForSeconds(0.5f);
                scrollScrapSwapTile.SetLightning(false, styleIndex: 2);
            }
        }
    }

    private void SetScrollScrapIconToShowSwapTile(ArtifactTileButton tile)
    {
        tile.SetLightning(true, styleIndex: 2);
        scrollScrapTabImage.sprite = tabSpritesArray[tile.x, tile.y];
    }
}
