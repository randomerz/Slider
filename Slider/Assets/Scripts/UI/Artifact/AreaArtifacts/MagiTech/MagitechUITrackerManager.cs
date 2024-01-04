using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiTechUITrackerManager : UITrackerManager
{
    private Vector2 pastCenter = new Vector2(117, 17);
    [SerializeField] private MagiTechArtifact magiTechArtifact;

    protected override Vector2 CalculateOffsetNullTile(UITracker tracker) 
    {
        Vector2 position = tracker.GetPosition();
        bool isTrackerInPast = position.x > 62.5;

        if(isTrackerInPast != magiTechArtifact.IsDisplayingPast())
            tracker.gameObject.SetActive(false);
        else
            tracker.gameObject.SetActive(true);

        Vector2 offset = (position - (isTrackerInPast ? pastCenter: center)) * centerScale;
        offset = new Vector3(Mathf.Clamp(offset.x, -70f, 70f), Mathf.Clamp(offset.y, -60f, 60f));
        return offset;
    }
}
