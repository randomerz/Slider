using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainGrid : SGrid
{
    public static MountainGrid instance;
    public Vector2Int gridOffset; //the offset between the bottom layer and top layer
    public STile[,] topGrid;
    public SGridBackground[,] topBgGrid;


    private new void Awake() {
        myArea = Area.Mountain;
        base.Awake();
        instance = this;
    }

    public STile[,] getTopGrid()
    {
        return topGrid;
    }

    public SGridBackground[,] getTopBGGrid()
    {
        return topBgGrid;
    }



}
