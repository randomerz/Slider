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

    public GameObject orbEast;
    public GameObject orbNorth;
    public GameObject orbWest;
    public GameObject orbSouth;


    public void SetBoat(bool value)
    {
        boatLeft.enabled = value;
        boatRight.enabled = value;
    }

    public void SetVolcanoEast(bool value)
    {
        volcanoEast.enabled = value;
        orbEast.SetActive(value);
        rockWest.enabled = value;
    }

    public void SetVolcanoNorth(bool value)
    {
        volcanoNorth.enabled = value;
        orbNorth.SetActive(value);
        rockSouth.enabled = value;
    }

    public void SetVolcanoWest(bool value)
    {
        volcanoWest.enabled = value;
        orbWest.SetActive(value);
        rockEast.enabled = value;
    }

    public void SetVolcanoSouth(bool value)
    {
        volcanoSouth.enabled = value;
        orbSouth.SetActive(value);
        rockNorth.enabled = value;
    }
}
