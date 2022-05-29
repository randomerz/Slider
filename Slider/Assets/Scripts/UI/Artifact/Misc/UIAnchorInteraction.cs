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
    }

    private void OnDisable()
    {
        Anchor.OnAnchorInteract -= OnAnchorInteract;
    }

    private void OnAnchorInteract(object sender, Anchor.OnAnchorInteractArgs interactArgs)
    {

        STile dropTile = interactArgs.stile;
        if(dropTile != null)
        {
            ArtifactTileButton anchorbutton = uiArtifactMenus.uiArtifact.GetButton(dropTile.islandId);
            anchorbutton.buttonAnimator.SetAnchored(interactArgs.drop);
        }
        

    }
    

}