using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackableItem : Item
{
    public bool trackerEnabled;

    public override void Start()
    {
        base.Start();

        if (trackerEnabled)
        {
            SetTrackerEnabled(trackerEnabled);
        }
    }

    public override void PickUpItem(Transform pickLocation, System.Action callback = null)
    {
        base.PickUpItem(pickLocation, callback);
        UITrackerManager.RemoveTracker(gameObject);
    }

    public override STile DropItem(Vector3 dropLocation, System.Action callback=null) 
    {
        if (trackerEnabled)
        {
            AddTracker();
        }
        return base.DropItem(dropLocation, callback);
    }

    public void SetTrackerEnabled(bool value) 
    {
        trackerEnabled = value;
        if (trackerEnabled)
        {
            AddTracker();
        }
        else
        {
            UITrackerManager.RemoveTracker(gameObject);
        }
    }

    private void AddTracker()
    {
        UITrackerManager.AddNewTracker(gameObject);
    }
}
