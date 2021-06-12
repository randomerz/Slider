using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactButton : MonoBehaviour
{
    public bool isEmpty = false;
    public bool isHighlighted = false;

    public int islandId = -1;
    public int xPos;
    public int yPos;

    private const int SLIDER_WIDTH = 17;

    public UIArtifact buttonManager;

    private void Awake()
    {
        // update artifact button
    }

    public void SelectButton()
    {
        buttonManager.SelectButton(this);
    }

    internal void SetHighlighted(bool v)
    {
        isHighlighted = true;
    }
}
