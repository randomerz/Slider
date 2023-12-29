using System.Collections.Generic;
using UnityEngine;

public class SGridJungleTilesExplored : SGridTilesExplored
{
    public override void SetTileExplored(Area area, int stileId, bool explored=true)
    {
        SaveSystem.Current.SetBool(BuildSaveString(area, stileId), explored);

        if (stileId == 2)
        {
            SaveSystem.Current.SetBool(BuildSaveString(area, 3), explored);
        }
        if (stileId == 3)
        {
            SaveSystem.Current.SetBool(BuildSaveString(area, 2), explored);
        }
    }
}