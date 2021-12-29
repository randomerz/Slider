using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactTileButtonAnimator : MonoBehaviour
{
    public Image sliderImage;
    public Image pushedDownFrame;
    public Image highlightedFrame;

    private bool isPushedDown;
    private bool isForcedPushedDown;
    private bool isHighlighted;

    //public void UpdatePushedDown()
    //{
    //    if (isPushedDown)
    //    {
    //        sliderImage.rectTransform.anchoredPosition = new Vector3(0, -1);
    //        highlightedFrame.rectTransform.anchoredPosition = new Vector3(0, -1);
    //        pushedDownFrame.gameObject.SetActive(true);
    //    }
    //    else
    //    {
    //        isPushedDown = false;
    //        sliderImage.rectTransform.anchoredPosition = new Vector3(0, 0);
    //        highlightedFrame.rectTransform.anchoredPosition = new Vector3(0, 0);
    //        pushedDownFrame.gameObject.SetActive(false);
    //    }
    //}

    public void SetPushedDown(bool value)
    {
        if (!isPushedDown && value)
        {
            isPushedDown = true;
            sliderImage.rectTransform.anchoredPosition = new Vector3(0, -1);
            highlightedFrame.rectTransform.anchoredPosition = new Vector3(0, -1);
            pushedDownFrame.gameObject.SetActive(true);
        }
        else if (isPushedDown && !value && !isForcedPushedDown)
        {
            isPushedDown = false;
            sliderImage.rectTransform.anchoredPosition = new Vector3(0, 0);
            highlightedFrame.rectTransform.anchoredPosition = new Vector3(0, 0);
            pushedDownFrame.gameObject.SetActive(false);
        }
    }

    public void SetForcedPushedDown(bool value)
    {
        isForcedPushedDown = value;
        SetPushedDown(value);
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
