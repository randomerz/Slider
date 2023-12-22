using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class UIAnchorInteraction : MonoBehaviour
{
    public UIArtifactMenus uiArtifactMenus;

    private void OnEnable()
    {
        Anchor.OnAnchorInteract += OnAnchorInteract;
        SGrid.OnSTileEnabled += OnSTileEnabled;
    }

    private void OnDisable()
    {
        Anchor.OnAnchorInteract -= OnAnchorInteract;
        SGrid.OnSTileEnabled -= OnSTileEnabled;
    }

    private void OnAnchorInteract(object sender, Anchor.OnAnchorInteractArgs interactArgs)
    {

        STile dropTile = interactArgs.stile;
        if(dropTile != null)
        {
            ArtifactTileButton anchorbutton = uiArtifactMenus.uiArtifact.GetButton(dropTile.islandId);
            if(anchorbutton != null)
            {
                anchorbutton.buttonAnimator.SetAnchored(interactArgs.drop);
                if (anchorbutton.LinkButton != null)
                {
                    anchorbutton.LinkButton.buttonAnimator.SetAnchored(interactArgs.drop);
                }
            }
        }
        

    }

    private void OnSTileEnabled(object sender, SGrid.OnSTileEnabledArgs args)
    {
        STile dropTile = args.stile;
        if(dropTile != null && dropTile.hasAnchor)
        {
            ArtifactTileButton anchorbutton = uiArtifactMenus.uiArtifact.GetButton(dropTile.islandId);
            if(anchorbutton != null)
            {
                anchorbutton.buttonAnimator.SetAnchored(true);
                if (anchorbutton.LinkButton != null)
                {
                    anchorbutton.LinkButton.buttonAnimator.SetAnchored(true);
                }
            }

        }
    }
    

}