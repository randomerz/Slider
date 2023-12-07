using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiTechUITrackerManager : UITrackerManager
{
    private Vector2 pastCenter = new Vector2(117, 17);

    protected override Vector2 CalculateOffsetNullTile(UITracker tracker) 
    {
        Vector2 position = tracker.GetPosition();
        Vector2 offset = (position - (position.x > 62.5? pastCenter: center)) * centerScale;
        offset = new Vector3(Mathf.Clamp(offset.x, -70f, 70f), Mathf.Clamp(offset.y, -60f, 60f));
        return offset;
    }

    //TODO: Make it so outside map tackers only show up when the correct time is shown on artifact map
}
