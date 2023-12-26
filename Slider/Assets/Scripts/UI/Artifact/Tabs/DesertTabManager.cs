using System.Collections;
using UnityEngine;

public class DesertTabManager : ArtifactTabManager
{
    [Header("Frag Realign Tab")]
    [SerializeField] private ArtifactTab fragRealignTab;
    [SerializeField] private Animator rearrangingFragTabAnimator;
    private ArtifactTileButton fragSwapTile;
    private ArtifactTileButton middle;

    private Coroutine fragSwapTileCycleRoutine;

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
        //UpdateMiddleAndFragSwapTile();
        //Mirage
        //MirageSTileManager.GetInstance().UpdateMirageSTileOnFrag(empty.x, empty.y);
        FragRearrangeOnHoverExit();
    }

    public void FragRearrangeOnHoverEnter()
    {
        rearrangingFragTabAnimator.SetFloat("speed", 4);
        //Preview!
        middle = UIArtifact.GetButton(1, 1);
        
        if (middle.TileIsActive)
        {
            UpdateFragSwapTile();
            UIArtifact.SetLightningPos(1, 1);
            middle.SetLightning(true);
            fragSwapTile.SetLightning(true);
        }
    }

    public void FragRearrangeOnHoverExit()
    {
        if (fragSwapTileCycleRoutine != null)
        {
            StopCoroutine(fragSwapTileCycleRoutine);
        }

        rearrangingFragTabAnimator.SetFloat("speed", 1);
        //Reset preview
        UIArtifact.DisableLightning(true);
        middle.SetLightning(false);
        if (fragSwapTile != null)
        {
            fragSwapTile.SetLightning(false);
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
                fragSwapTile.SetLightning(true);
                yield return new WaitForSeconds(1);
                fragSwapTile.SetLightning(false);
            }
        }
    }
}
