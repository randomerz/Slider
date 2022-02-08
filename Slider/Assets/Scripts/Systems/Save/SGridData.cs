using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SGridData 
{
    public List<STileData> grid = new List<STileData>();
    // public List<STileData> altGrid = new List<STileData>();

    [System.Serializable]
    public class STileData {
        
        public bool isTileActive;
        public int islandId;
        public int x;
        public int y;

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
        foreach (STile s in sgrid.GetGrid()) {
            grid.Add(new STileData(s));
        }
        // if (sgrid.GetAltGrid() != null) {
        //     foreach (STile s in sgrid.GetAltGrid()) {
        //         altGrid.Add(new STileData(s));
        //     }
        // }
    }
}