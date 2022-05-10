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

    protected override void Awake() {
        myArea = Area.Mountain;

        foreach (Collectible c in collectibles)
        {
            c.SetArea(myArea);
        }

        base.Awake();

        instance = this;
    }


    protected override void Start()
    {
        base.Start();

        AudioManager.PlayMusic("Mountain");
        UIEffects.FadeFromBlack();

    }
}