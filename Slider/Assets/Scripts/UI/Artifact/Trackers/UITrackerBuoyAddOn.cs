using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITrackerBuoyAddOn : MonoBehaviour
{
    public RectTransform myRectTransform;
    public RectTransform target1;
    public RectTransform target2;
    public RectTransform pointer1;
    public RectTransform pointer2;

    private void Update() 
    {
        if (target1 == null || target2 == null)
        {
            Debug.LogError("Targets for Buoy add on were not set!");
            return;
        }

        Vector2 dif = target1.position - myRectTransform.position;
        float a = Mathf.Atan2(dif.y, dif.x);
        pointer1.rotation = Quaternion.Euler(0, 0, a * Mathf.Rad2Deg);

        dif = target2.position - myRectTransform.position;
        a = Mathf.Atan2(dif.y, dif.x);
        pointer2.rotation = Quaternion.Euler(0, 0, a * Mathf.Rad2Deg);
    }
}
