using UnityEngine;

public class DesertTabManager : ArtifactTabManager
{
    [Header("Frag Realign Tab")]
    [SerializeField] private ArtifactTab fragRealignTab;
    [SerializeField] private Animator rearrangingFragTabAnimator;
    private ArtifactTileButton empty;
    private ArtifactTileButton middle;
    public override void SetCurrentScreen(int screenIndex)
    {
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
        }
    }

    public void FragRearrangeOnClick()
    {
        // Do the rearranging!
        //Debug.Log("Swapped!");
        if (middle == empty)
        {
            AudioManager.Play("Artifact Error");
            return;
        }
        DesertArtifact artifact = (DesertArtifact)uiArtifactMenus.uiArtifact;
        artifact.TryFragQueueMoveFromButtonPair(middle, empty);
        artifact.UpdatePushedDowns(null, null);
        artifact.DeselectSelectedButton();
        //FragRearrangeOnHoverExit();
    }

    public void FragRearrangeOnHoverEnter()
    {
        rearrangingFragTabAnimator.SetFloat("speed", 4);
        //Preview!
        middle = UIArtifact.GetButton(1, 1);
        empty = uiArtifactMenus.uiArtifact.GetButton(9);
        if (middle.TileIsActive) UIArtifact.SetLightningPos(1, 1);
        middle.SetLightning(true);
        empty.SetLightning(true);
    }

    public void FragRearrangeOnHoverExit()
    {
        rearrangingFragTabAnimator.SetFloat("speed", 1);
        //Reset preview
        UIArtifact.DisableLightning(true);
        middle.SetLightning(false);
        empty.SetLightning(false);
    }
}
