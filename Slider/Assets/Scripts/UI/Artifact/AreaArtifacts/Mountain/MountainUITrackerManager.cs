using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainUITrackerManager : UITrackerManager
{
    public static MountainUITrackerManager _instance;

    private Vector2 xOffset = new Vector2(35f/50f, -16f/50f);
    private Vector2 yOffset = new Vector2(35f/50f, 16f/50f);
    private Vector2 lowerCenter;
    private Vector2 upperCenter;

    protected override void Awake() {
        base.Awake();
        _instance = this;
    }

    /*protected override void calculateOffsetNullTile() 
    {
        offset = (position - center) * centerScale;
        //yea i have no idea how to deal with that yet 
        //offset = new Vector3(Mathf.Clamp(offset.x, -62.5f, 62.5f), Mathf.Clamp(offset.y, -57.5f, 57.5f));
    }*/

    protected override void calculateOffset() 
    {
        Debug.Log("sussy baka");
        Vector2 temp =(position - (Vector2)currentTile.transform.position);
        offset = temp.x * xOffset + temp.y * yOffset;
    }
}
