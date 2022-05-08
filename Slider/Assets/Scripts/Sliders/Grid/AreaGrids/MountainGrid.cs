using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainGrid : SGrid
{
    public int layerOffset; //the y offset of the top layer from the bottom (used to calculate top tile y position)

    /* The mountian sgrid is a 2 by 4 grid. The top 4 tiles represent the top layer,
        while the bottom 4 tiles represent the bottom layer. For example, the following grid

                5 1
                2 8

                4 7
    (0,0) ->    6 3

        represents the grid with 5, 1, 2, and 8 on the top layer.
        In order to accomidate this grid, the following major changes have been made
        1. There is a custom logic for determining a tile's neighbors
        2. There is a new MountainSGridAnimator which allows layer swaps to happen without animation
        3. The bottom 4 sTile locations have cameras above them, allowing them to be shown on textures "underneath" the top tiles
        4. 
    */


    protected override void Awake() {
        myArea = Area.Mountain;
        base.Awake();
    }
}