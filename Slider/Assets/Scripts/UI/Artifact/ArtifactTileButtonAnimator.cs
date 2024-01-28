using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactTileButtonAnimator : MonoBehaviour
{
    public Image sliderImage;
    public Image pushedDownFrame;
    public Image highlightedFrame;
    public Image controllerHoverFrame;

    //The button is pushed down, regardless of the method
    private bool isPushedDown;
    //The button is selected by the user to be able to make a move.
    [SerializeField] private bool isSelected;
    //The button is forced down because it is still being moved or is anchored.
    [SerializeField] private bool isForcedDown;
    private bool isHighlighted;
    //Button has lightning highlight and pusheddown and has lightning effect around it
    [SerializeField] private bool isLightning;
    //Button is highlighted by desert scroll
    private bool isScrollHighlighted;

    private Coroutine positionAnimatorCoroutine;
    [SerializeField] private float positionAnimationDuration;
    [SerializeField] private AnimationCurve positionAnimationCurve;

    [Header("Alt Styles")] // 0 - Normal, 1 - lightning, 2 - scroll
    [SerializeField] private List<Image> frames;
    [SerializeField] private List<Image> borders;

    public bool IsPushedDown
    {
        get
        {
            return isPushedDown;
        }
        private set
        {
            isPushedDown = value;
        }
    }

    public void SetPushedDown(bool value)
    {
        value = value || isForcedDown;
        if (!isPushedDown && value)
        {
            isPushedDown = true;
            sliderImage.rectTransform.anchoredPosition = new Vector3(0, -1);
            highlightedFrame.rectTransform.anchoredPosition = new Vector3(0, -1);
            pushedDownFrame.gameObject.SetActive(true);
            // if (!isLightning) highlightedFrame.gameObject.SetActive(false); //We don't disable the highlight if lightning
        }
        else if (isPushedDown && !value)
        {
            isPushedDown = false;
            sliderImage.rectTransform.anchoredPosition = new Vector3(0, 0);
            highlightedFrame.rectTransform.anchoredPosition = new Vector3(0, 0);
            foreach (Image i in frames)
            {
                i.gameObject.SetActive(false);
            }
            SetHighlighted(isLightning); //Edge case where you set lightning while tile is moving. Needed for desert frag
            SetHighlighted(isScrollHighlighted); //needed for desert frag?
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

    public void SetControllerHoverHighlight(bool value, bool log=false)
    {
        controllerHoverFrame.gameObject.SetActive(value);
    }

    public void SetHighlighted(bool value)
    {
        if (!isHighlighted && value)
        {
            isHighlighted = true;
            highlightedFrame.gameObject.SetActive(true);
        }
        else if (isHighlighted && !value && !isLightning && !isScrollHighlighted) //If lightning is active, tile should never be unhighlighted
        {
            isHighlighted = false;
            foreach (Image i in borders)
            {
                i.gameObject.SetActive(false);
            }
        }
    }

    public void SetLightning(bool value)
    {
        if (!isLightning && value)
        {
            highlightedFrame.gameObject.SetActive(false);
            pushedDownFrame.gameObject.SetActive(false);
            Image lightningPushedDown = frames[1];
            Image lightningHighlight = borders[1];
            pushedDownFrame = lightningPushedDown;
            highlightedFrame = lightningHighlight;
            if (isForcedDown)
            {
                pushedDownFrame.gameObject.SetActive(true);
            }
            else
            {
                pushedDownFrame.gameObject.SetActive(false);
                highlightedFrame.gameObject.SetActive(true);//When lightning is active, tile should always be highlighted
            }
        }
        else if (isLightning && !value)
        {
            highlightedFrame.gameObject.SetActive(false);
            pushedDownFrame.gameObject.SetActive(false);
            Image PushedDown = frames[0];
            Image Highlighted = borders[0];
            pushedDownFrame = PushedDown;
            highlightedFrame = Highlighted;
            if (isPushedDown) pushedDownFrame.gameObject.SetActive(true);
        }
        isLightning = value;
    }

    public void SetScrollHighlight(bool value)
    {
        if (!isScrollHighlighted && value)
        {
            highlightedFrame.gameObject.SetActive(false);
            pushedDownFrame.gameObject.SetActive(false);
            Image scrollPushedDown = frames[2];
            Image scrollHighlight = borders[2];
            pushedDownFrame = scrollPushedDown;
            highlightedFrame = scrollHighlight;
            if (isForcedDown)
            {
                pushedDownFrame.gameObject.SetActive(true);
            }
            else
            {
                pushedDownFrame.gameObject.SetActive(false);
                highlightedFrame.gameObject.SetActive(true);//When scroll is active, tile should always be highlighted
            }
        }
        else if (isScrollHighlighted && !value)
        {
            highlightedFrame.gameObject.SetActive(false);
            pushedDownFrame.gameObject.SetActive(false);
            Image PushedDown = frames[0];
            Image Highlighted = borders[0];
            pushedDownFrame = PushedDown;
            highlightedFrame = Highlighted;
            if (isPushedDown) pushedDownFrame.gameObject.SetActive(true);
        }
        isScrollHighlighted = value;
    }

    public void AnimatePositionFrom(Vector2 position)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        // In cases where object spawns and moves before artifact is opened, i.e. Loading game into Factory conveyors
        if (!gameObject.activeInHierarchy)
            return;
        if (positionAnimatorCoroutine != null)
        {
            StopCoroutine(positionAnimatorCoroutine);
        }

        positionAnimatorCoroutine = StartCoroutine(AnimatePosition(position, Vector2.zero));
    }

    private IEnumerator AnimatePosition(Vector2 from, Vector2 to)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        float t = 0;
        while (t < positionAnimationDuration)
        {
            float c = positionAnimationCurve.Evaluate(t / positionAnimationDuration);
            Vector2 pos = Vector2.Lerp(from, to, c);

            rectTransform.anchoredPosition = pos;

            yield return null;
            t += Time.deltaTime;
        }

        rectTransform.anchoredPosition = to;
        positionAnimatorCoroutine = null;
    }
}
