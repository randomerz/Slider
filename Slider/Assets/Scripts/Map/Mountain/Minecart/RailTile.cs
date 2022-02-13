using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Tilemaps;

public class RailTile : Tile
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private bool state {get; set;}
    /*Represents connections between points. ENWS = 0123. -1 is used for an unreachable direction. The first index represents the state
      IE [[1,3,-1,1][1,0,-1,1]] means that E->N, N->S, and S->N in state 0 and E->N, N->E, and S->N in stage 1   
    */
    [SerializeField] private int[,] indexMap = new int[2,4];


    [SerializeField] private RailTile[] adjTiles;

}
