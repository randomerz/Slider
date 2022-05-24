using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertArtifactRandomizer
{
    public static void ShuffleGrid()
    {
        string[] puzzle = { "165897432", "429185637", "253917468", "651329748", "912345678", 
            "985764321", "123456798", "132457869", "491326857", "856372941",
            "394821765", "645782913", "746918253", "953186742", "281936457",
            "319754826", "791825643", "834597621", "241936578", "485691237"};
        int random = Random.Range(0, 20);
        Debug.Log("Shuffled Puzzle Index: " + random);
        SGrid.current.SetGrid(SGrid.GridStringToSetGridFormat(puzzle[random]));
    }
}
