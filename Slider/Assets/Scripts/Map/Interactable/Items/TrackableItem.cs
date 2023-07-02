using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackableItem : Item
{
    private bool trackerEnabled;

    public override void PickUpItem(Transform pickLocation, System.Action callback = null)
    {
        base.PickUpItem(pickLocation, callback);
        UITrackerManager.RemoveTracker(this.gameObject);
    }

    public override STile DropItem(Vector3 dropLocation, System.Action callback=null) 
    {
        if(trackerEnabled)
            UITrackerManager.AddNewTracker(this.gameObject);
        return base.DropItem(dropLocation, callback);
    }

    public void SetTrackerEnabled(bool value) 
    {
        trackerEnabled = value;
        if(!trackerEnabled)
            UITrackerManager.RemoveTracker(this.gameObject);
        if(trackerEnabled)
            UITrackerManager.AddNewTracker(this.gameObject);
    }
}
