using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertArtifactRandomizer
{
    public static void ShuffleGrid()
    {
        string[] puzzle = { "165793482", "429175683", "258913467", "651829347", "912845637",
            "975364821", "128456397", "182453769", "491826753", "756832941",
            "894721365", "645372918", "346917258", "958176342", "271986453",
            "819354726", "391725648", "784593621", "241986537", "475691283"};
        int random = Random.Range(0, 20);
        Debug.Log("Shuffled Puzzle Index: " + random);
        SGrid.Current.SetGrid(SGrid.GridStringToSetGridFormat(puzzle[random]));
    }
}
