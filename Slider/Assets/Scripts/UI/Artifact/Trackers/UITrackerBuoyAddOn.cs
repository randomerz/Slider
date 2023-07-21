using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITrackerBuoyAddOn : MonoBehaviour
{
    public RectTransform myRectTransform;
    public RectTransform target1;
    public RectTransform target2;
    public RectTransform pointer1;
    public RectTransform pointer2;
    public Image image1;
    public Image image2;
    public KnotBox knotBox;
    public int index1;
    public int index2;

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
        image1.color = index1 < knotBox.linesArr.Length && knotBox.linesArr[index1] ? new Color(0.101960784f, 0.101960784f, 0.101960784f, 1) : new Color(0.6f, 0.6f, 0.6f, 1);

        dif = target2.position - myRectTransform.position;
        a = Mathf.Atan2(dif.y, dif.x);
        pointer2.rotation = Quaternion.Euler(0, 0, a * Mathf.Rad2Deg);
        image2.color = index2 < knotBox.linesArr.Length && knotBox.linesArr[index2] ? new Color(0.101960784f, 0.101960784f, 0.101960784f, 1) : new Color(0.6f, 0.6f, 0.6f, 1);
    }
}
