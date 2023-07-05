using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainUITrackerManager : UITrackerManager
{
#pragma warning disable
    public static MountainUITrackerManager _instance;
#pragma warning restore

    private Vector2 xOffset = new Vector2(40f/34f, -18f/34f);
    private Vector2 yOffset = new Vector2(40f/34f, 18f/34f);
    private Vector2 xCenterOffset = new Vector2(53f/34f, -29f/34f);
    private Vector2 yCenterOffset = new Vector2(53f/34f, 29f/34f);
    private Vector2 lowerCenter = new Vector2(8.5f, 8.5f);
    private Vector2 upperCenter = new Vector2(8.5f, 108.5f);

    protected override void Awake() {
        base.Awake();
        _instance = this;
    }

    protected override Vector2 CalculateOffsetNullTile(UITracker tracker) 
    {
        Vector2 position = tracker.GetPosition();
        Vector2 temp = (position - (position.y > 62.5? upperCenter: lowerCenter));
        temp.x = ScaleOutsideCutoff(temp.x, -17, 17, .5f);
        temp.y = ScaleOutsideCutoff(temp.y, -17, 17, .5f);
        Vector2 offset = temp.x * xCenterOffset + temp.y * yCenterOffset;
        offset += (position.y > 62.5 ? new Vector2(0, 29.5f) : new Vector2(0, -29.5f));
        return offset;
    }

    protected override Vector2 CalculateOffset(UITracker tracker) 
    {
        Vector2 position = tracker.GetPosition();
        Vector2 temp = (position - (Vector2)currentTile.transform.position);
        Vector2 offset = temp.x * xOffset + temp.y * yOffset;
        return offset;
    }

    //multiplies the part of num greater than upperCutoff or less than lowerCutoff by scaleFactor
    //Example: (20, -5, 5, .5) 
    //20 > 5     20-5 = 15    15 * .5 = 7.5    5 + 7.5 = 12.5
    private float ScaleOutsideCutoff(float num, float lowerCutoff, float upperCutoff, float scaleFactor)
    {
        if(num > upperCutoff) {
            float excess = num - upperCutoff;
            return upperCutoff + (excess * scaleFactor);
        }
        else if(num < lowerCutoff) {
            float excess = lowerCutoff - num;
            return lowerCutoff - (excess * scaleFactor);
        }
        else return num;
       
    }
}
