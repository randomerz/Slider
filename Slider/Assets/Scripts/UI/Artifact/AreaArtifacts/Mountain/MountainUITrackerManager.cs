using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainUITrackerManager : UITrackerManager
{
    public static MountainUITrackerManager _instance;

    private Vector2 xOffset = new Vector2(35f/50f, -16f/50f);
    private Vector2 yOffset = new Vector2(35f/50f, 16f/50f);
    private Vector2 xCenterOffset = new Vector2(53f/50f, -29f/50f);
    private Vector2 yCenterOffset = new Vector2(53f/50f, 29f/50f);
    private Vector2 lowerCenter = new Vector2(12.5f, 12.5f);
    private Vector2 upperCenter = new Vector2(12.5f, 112.5f);

    protected override void Awake() {
        base.Awake();
        _instance = this;
    }

    protected override void CalculateOffsetNullTile() 
    {
        Vector2 temp = (position - (position.y > 62.5? upperCenter: lowerCenter));
        offset = temp.x * xCenterOffset + temp.y * yCenterOffset + (position.y > 62.5? new Vector2(0, 29): new Vector2(0, -30));
        //C: TODO: clamp outside of stiles
        //offset = new Vector3(Mathf.Clamp(offset.x, -62.5f, 62.5f), Mathf.Clamp(offset.y, -57.5f, 57.5f));
    }

    protected override void CalculateOffset() 
    {
        Vector2 temp = (position - (Vector2)currentTile.transform.position);
        offset = temp.x * xOffset + temp.y * yOffset;
    }
}
