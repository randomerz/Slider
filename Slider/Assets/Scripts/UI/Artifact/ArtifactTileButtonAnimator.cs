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
    //Button has lightning highlight and pusheddown and has lightning effect around it
    [SerializeField] private bool isLightning;

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

    public void SetIsForcedDown(bool value)
    {
        isForcedDown = value;
        SetPushedDown(isSelected);
    }

    public void SetAnchored(bool value)
    {
        isForcedDown = value;
        SetPushedDown(isForcedDown);
    }

    public void SetHighlighted(bool value)
    {
        if (!isHighlighted && value)
        {
            isHighlighted = true;
            highlightedFrame.gameObject.SetActive(true);
        }
        else if (isHighlighted && !value && !isLightning)
        {
            isHighlighted = false;
            highlightedFrame.gameObject.SetActive(false);
        }
    }

    public void SetLightning(bool value)
    {
        if (!isLightning && value)
        {
            highlightedFrame.gameObject.SetActive(false);
            Image lightningPushedDown = this.gameObject.transform.GetChild(1).GetChild(1).GetComponent<Image>();
            Image lightningHighlight = this.gameObject.transform.GetChild(2).GetChild(1).GetComponent<Image>();
            pushedDownFrame = lightningPushedDown;
            highlightedFrame = lightningHighlight;
            isHighlighted = true; //When lightning is active, tile should always be highlighted
            highlightedFrame.gameObject.SetActive(true);
        }
        else if (isLightning && !value)
        {
            highlightedFrame.gameObject.SetActive(false);
            Image PushedDown = this.gameObject.transform.GetChild(1).GetChild(0).GetComponent<Image>();
            Image Highlighted = this.gameObject.transform.GetChild(2).GetChild(0).GetComponent<Image>();
            pushedDownFrame = PushedDown;
            highlightedFrame = Highlighted;
        }
        isLightning = value;
    }
    //Chen: This just enables the lightning highlights for use in Desert
    public void FragLightningPreview(bool value)
    {
        this.gameObject.transform.GetChild(2).GetChild(1).gameObject.SetActive(value);
    }
}
