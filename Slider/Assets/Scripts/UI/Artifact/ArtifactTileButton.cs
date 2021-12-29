using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactTileButton : MonoBehaviour
{
    public static bool canComplete = false;
    public bool isComplete = false;

    public bool isTileActive = false;
    public int islandId = -1;
    public int x;
    public int y;

    private const int UI_OFFSET = 37;

    private STile myStile;
    private Sprite islandSprite;
    public Sprite completedSprite;
    public Sprite emptySprite;
    public ArtifactTileButtonAnimator buttonAnimator;
    public UIArtifact buttonManager;

    private void Start()
    {
        islandSprite = buttonAnimator.sliderImage.sprite;

        myStile = SGrid.GetStile(islandId); // happens in SGrid.Awake()
        SetTileActive(myStile.isTileActive);
        SetPosition(myStile.x, myStile.y);

        //if (!isTileActive)
        //{
        //    buttonAnimator.sliderImage.sprite = emptySprite;
        //}
        // update artifact button
    }

    public void SetPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
        Vector3 pos = new Vector3(x - 1, y - 1) * UI_OFFSET;
        GetComponent<RectTransform>().anchoredPosition = pos;
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

    public void SetTileActive(bool v)
    {
        isTileActive = v;
        if (v)
        {
            // animation?
            buttonAnimator.sliderImage.sprite = islandSprite;
        }
        else
        {
            buttonAnimator.sliderImage.sprite = emptySprite;
        }
    }

    public void SetComplete(bool value)
    {
        if (!canComplete || isTileActive)
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
