using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactButton : MonoBehaviour
{
    public static bool canComplete = false;
    public bool isComplete = false;
    public bool isEmpty = false;

    public int islandId = -1;
    public int xPos;
    public int yPos;

    private const int SLIDER_OFFSET = 37;

    private TileSlider mySlider;
    private Sprite islandSprite;
    public Sprite completedSprite;
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

    //public void UpdatePushedDown()
    //{
    //    buttonAnimator.UpdatePushedDown();
    //}

    public void SetHighlighted(bool v)
    {
        buttonAnimator.SetHighlighted(v);
    }

    public void SetPushedDown(bool v)
    {
        buttonAnimator.SetPushedDown(v);
    }

    public void SetForcedPushedDown(bool v)
    {
        buttonAnimator.SetForcedPushedDown(v);
    }

    public void SetEmpty(bool v)
    {
        isEmpty = v;
        if (v)
        {
            buttonAnimator.sliderImage.sprite = emptySprite;
        }
        else
        {
            // animation?
            buttonAnimator.sliderImage.sprite = islandSprite;
        }
    }

    public void SetComplete(bool value)
    {
        if (!canComplete)
            return;

        isComplete = value;
        if (isComplete)
        {
            buttonAnimator.sliderImage.sprite = completedSprite;
        }
        else
        {
            buttonAnimator.sliderImage.sprite = islandSprite;
        }
    }
}
