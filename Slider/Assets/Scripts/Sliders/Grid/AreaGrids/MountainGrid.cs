using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainGrid : SGrid
{
    public int layerOffset; //the y offset of the top layer from the bottom (used to calculate top tile y position)

    public static MountainGrid instance;

    /* The mountian sgrid is a 2 by 4 grid. The top 4 tiles represent the top layer,
        while the bottom 4 tiles represent the bottom layer. For example, the following grid

                5 1
                2 8

                4 7
    (0,0) ->    6 3

        represents the grid with 5, 1, 2, and 8 on the top layer.
    */

    public override void Init() {
        myArea = Area.Mountain;

        foreach (Collectible c in collectibles)
        {
            c.SetArea(myArea);
        }

        base.Init();
        instance = this;
    }
    
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
        if (interactArgs.drop)
        {
            STile dropTile = interactArgs.stile;
            if(dropTile!= null)
            {
                if(dropTile.y < 2)
                return; //currently using the anchor on the bottom layer does nothing
                STile lower = SGrid.current.GetGrid()[dropTile.x, dropTile.y - 2];
                if(!lower.isTileActive)  //if this is true, then there is not an active tile below the current tile
                {
                    MountainArtifact uiArtifact = (MountainArtifact) MountainArtifact.GetInstance();
                    UIArtifact.ClearQueues();
                    uiArtifact.AnchorSwap(dropTile, lower);
                }
            }
            
        }
        //PJ: define behavior for anchor pick up?
        
    }

    protected override void Start()
    {
        base.Start();
        AudioManager.PlayMusic("Mountain");
        UIEffects.FadeFromBlack();

    }
}