using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactButton : MonoBehaviour
{
    public bool isEmpty = false;

    public int islandId = -1;
    public int xPos;
    public int yPos;

    private const int SLIDER_OFFSET = 37;

    private TileSlider mySlider;
    private Sprite islandSprite;
    public Sprite emptySprite;
    public ArtifactButtonAnimator buttonAnimator;
    public UIArtifact buttonManager;

    private void Awake()
    {
        // this should work
        mySlider = EightPuzzle.GetInstance().sliders[islandId - 1];
        isEmpty = mySlider.isEmpty;

        islandSprite = buttonAnimator.sliderImage.sprite;
        if (isEmpty)
        {
            buttonAnimator.sliderImage.sprite = emptySprite;
        }
        // update artifact button
    }

    public void SetPosition(int x, int y)
    {
        xPos = x;
        yPos = y;
        GetComponent<RectTransform>().anchoredPosition = new Vector3(x * SLIDER_OFFSET, y * SLIDER_OFFSET);
    }

    public void SelectButton()
    {
        buttonManager.SelectButton(this);
    }

    public void SetHighlighted(bool v)
    {
        buttonAnimator.SetHighlighted(v);
    }

    public void SetPushedDown(bool v)
    {
        buttonAnimator.SetPushedDown(v);
    }
}
