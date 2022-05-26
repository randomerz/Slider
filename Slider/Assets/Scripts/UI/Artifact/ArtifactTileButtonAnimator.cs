using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactTileButtonAnimator : MonoBehaviour
{
    public Image sliderImage;
    public Image pushedDownFrame;
    public Image highlightedFrame;

    //The button is pushed down, regardless of the method
    private bool isPushedDown;
    //The button is selected by the user to be able to make a move.
    [SerializeField] private bool isSelected;
    //The button is forced down because it is still being moved or is anchored.
    [SerializeField] private bool isForcedDown;
    private bool isHighlighted;

    public void SetPushedDown(bool value)
    {
        value = value || isForcedDown;
        if (!isPushedDown && value)
        {
            isPushedDown = true;
            sliderImage.rectTransform.anchoredPosition = new Vector3(0, -1);
            highlightedFrame.rectTransform.anchoredPosition = new Vector3(0, -1);
            pushedDownFrame.gameObject.SetActive(true);
        }
        else if (isPushedDown && !value)
        {
            isPushedDown = false;
            sliderImage.rectTransform.anchoredPosition = new Vector3(0, 0);
            highlightedFrame.rectTransform.anchoredPosition = new Vector3(0, 0);
            pushedDownFrame.gameObject.SetActive(false);
        }
        
        
    }

    public void SetSelected(bool value)
    {
        isSelected = value;
        SetPushedDown(isSelected);
    }
    public void SetIsAnchored(bool value)
    {
        isForcedDown = value;
        SetPushedDown(value);
    }

    public void SetIsForcedDown(bool value)
    {
        isForcedDown = value;
        SetPushedDown(isSelected);
    }

    public void SetHighlighted(bool value)
    {
        if (!isHighlighted && value)
        {
            isHighlighted = true;
            highlightedFrame.gameObject.SetActive(true);
        }
        else if (isHighlighted && !value)
        {
            isHighlighted = false;
            highlightedFrame.gameObject.SetActive(false);
        }
    }
}
