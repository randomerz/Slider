using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class AnchorUIEffects : MonoBehaviour 
{   
    public UIArtifactMenus uiArtifactMenus;
    private void OnEnable()
    {
        Anchor.OnAnchorDrop += OnAnchorDrop;
        Anchor.OnAnchorPickUp += OnAnchorPickUp;
    }

    private void OnDisable()
    {
        Anchor.OnAnchorDrop -= OnAnchorDrop;
        Anchor.OnAnchorPickUp -= OnAnchorPickUp;
    }

    private void OnAnchorDrop(object sender, Anchor.OnAnchorDropArgs dropArgs)
    {
        if(dropArgs.stile != null)
        {
            uiArtifactMenus.uiArtifact.anchoredTile = dropArgs.stile.islandId;
            uiArtifactMenus.uiArtifact.GetButton(uiArtifactMenus.uiArtifact.anchoredTile).buttonAnimator.SetIsAnchored(true);
            Debug.Log("Artifact tile detected anchor placed");
        }
            
    }
    private void OnAnchorPickUp(object sender, Anchor.OnAnchorPickUpArgs PickUpArgs)
    {
        if(PickUpArgs.stile != null)
        {
            uiArtifactMenus.uiArtifact.anchoredTile = PickUpArgs.stile.islandId;
            uiArtifactMenus.uiArtifact.GetButton(uiArtifactMenus.uiArtifact.anchoredTile).buttonAnimator.SetIsAnchored(false);
            Debug.Log("Artifact tile detected anchor removed");
        }
    }
}