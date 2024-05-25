using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryUITrackerManager : UITrackerManager
{
    protected override Vector2 center => new Vector2(19.5f, 19.5f);
    protected override float centerScale => 2; // idk why 2 looks best

    [Header("Military")]
    private static MilitarySpriteTable _militarySpriteTableCache;
    private static MilitarySpriteTable MilitarySpriteTable {
        get {
            if (_militarySpriteTableCache == null)
            {
                _militarySpriteTableCache = (MilitaryGrid.Current as MilitaryGrid).militarySpriteTable;
            }
            return _militarySpriteTableCache;
        }
    }

    public static void AddUnitTracker(MilitaryUnit unit)
    {
        AddNewTracker(
            unit.NPCController.gameObject,
            MilitarySpriteTable.GetUIIconForUnit(unit)
        );
    }

    public static void RemoveUnitTracker(MilitaryUnit unit)
    {
        RemoveTracker(unit.NPCController.gameObject);
    }
}
