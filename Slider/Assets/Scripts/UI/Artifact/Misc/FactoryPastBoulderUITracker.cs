using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactoryPastBoulderUITracker : MonoBehaviour
{
    public Image image;
    public ExplodableRock explodableRock;

    private bool isTrackerVisible;

    private void OnEnable()
    {
        UpdateIcon();
    }

    private bool ShouldTrackerBeVisible()
    {
        return FactoryGrid.PlayerInPast && !explodableRock.isExploded;
    }

    private void UpdateIcon()
    {
        isTrackerVisible = ShouldTrackerBeVisible();
        image.enabled = isTrackerVisible;
    }
}
