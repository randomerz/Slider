using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OceanArtifactHighlights : MonoBehaviour
{
    public Image boatLeft;
    public Image boatRight;

    public Image rockEast;
    public Image rockNorth;
    public Image rockWest;
    public Image rockSouth;
    public Image volcanoEast;
    public Image volcanoNorth;
    public Image volcanoWest;
    public Image volcanoSouth;

    public void SetBoat(bool value)
    {
        boatLeft.enabled = value;
        boatRight.enabled = value;
    }

    public void SetVolcanoEast(bool value)
    {
        volcanoEast.enabled = value;
        rockWest.enabled = value;
    }

    public void SetVolcanoNorth(bool value)
    {
        volcanoNorth.enabled = value;
        rockSouth.enabled = value;
    }

    public void SetVolcanoWest(bool value)
    {
        volcanoWest.enabled = value;
        rockEast.enabled = value;
    }

    public void SetVolcanoSouth(bool value)
    {
        volcanoSouth.enabled = value;
        rockNorth.enabled = value;
    }
}
