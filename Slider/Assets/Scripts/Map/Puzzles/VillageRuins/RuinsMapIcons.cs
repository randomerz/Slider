using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RuinsMapIcons : MonoBehaviour
{
    public Image corner1;
    public Image corner2;
    public Image corner3;
    public Image corner6;

    public void SetMapIcon(int index, bool value)
    {
        switch (index)
        {
            case 1:
                corner1.enabled = value;
                break;
            case 2:
                corner2.enabled = value;
                break;
            case 3:
                corner3.enabled = value;
                break;
            case 6:
                corner6.enabled = value;
                break;
        }
    }

    public void ResetIcons()
    {
        corner1.enabled = false;
        corner2.enabled = false;
        corner3.enabled = false;
        corner6.enabled = false;
    }
}
