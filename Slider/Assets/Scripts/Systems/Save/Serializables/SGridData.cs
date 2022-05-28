using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SGridData 
{
    public List<STileData> grid = new List<STileData>();
    // public List<STileData> altGrid = new List<STileData>();
    public int[,] realigningGrid;

    [System.Serializable]
    public class STileData {
        
        public bool isTileActive;
        public int islandId;
        public int x;
        public int y;
        public int z;

        public STileData(STile stile) {
            isTileActive = stile.isTileActive;
            islandId = stile.islandId;
            x = stile.x;
            y = stile.y;
        }
    }


    public SGridData(SGrid sgrid) {
        UpdateGrid(sgrid);
    }

    public void UpdateGrid(SGrid sgrid) {
        grid.Clear();
        for (int x = 0; x < sgrid.width; x++)
        {
            for (int y = 0; y < sgrid.height; y++)
            {
                STileData std = new STileData(sgrid.GetGrid()[x, y]);
                std.x = x;
                std.y = y;
                grid.Add(std);
            }
        }
        realigningGrid = sgrid.realigningGrid;
        // if (sgrid.GetAltGrid() != null) {
        //     foreach (STile s in sgrid.GetAltGrid()) {
        //         altGrid.Add(new STileData(s));
        //     }
        // }
    }
}