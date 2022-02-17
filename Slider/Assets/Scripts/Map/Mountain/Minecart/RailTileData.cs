using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RailTileData : MonoBehaviour
{
    public Sprite[] sprites = new Sprite[2];
    public int state;
    public int[,] indexMap = new int[2,4];
}
