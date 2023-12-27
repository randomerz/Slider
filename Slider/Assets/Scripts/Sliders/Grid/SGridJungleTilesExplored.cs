using System.Collections.Generic;
using UnityEngine;

public class SGridJungleTilesExplored : SGridTilesExplored
{
    public override void SetTileExplored(Area area, int stileId)
    {
        SaveSystem.Current.SetBool(BuildSaveString(area, stileId), true);

        if (stileId == 2)
        {
            SaveSystem.Current.SetBool(BuildSaveString(area, 3), true);
        }
        if (stileId == 3)
        {
            SaveSystem.Current.SetBool(BuildSaveString(area, 2), true);
        }
    }
}