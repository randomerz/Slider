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
    private bool isSelected;
    //The button is forced down because it is still being moved or is anchored.
    private bool isForcedDown;
    private bool isHighlighted;
    private bool isForcedHighlighted;
    //Button has lightning highlight and pusheddown and has lightning effect around it
    private bool isLightning;

    private Coroutine positionAnimatorCoroutine;
    private Vector2 positionAnimationFinalPosition;
    [SerializeField] private float positionAnimationDuration;
    [SerializeField] private AnimationCurve positionAnimationCurve;

    [Header("Alt Styles")] // 0 - Normal, 1 - lightning, 2 - scroll
    [SerializeField] private List<Image> frames;
    [SerializeField] private List<Image> borders;

    public bool IsPushedDown
    {
        get => isPushedDown;
        private set { isPushedDown = value; }
    }

    private void Start() 
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        positionAnimationFinalPosition = rectTransform.anchoredPosition;
    }

    private void OnDisable() 
    {
        if (positionAnimationCurve != null)
        {
            AnimatePositionEnd();
        }
    }

    public void SetPushedDown(bool value)
    {
        value = value || isForcedDown;
        if (!isPushedDown && value)
        {
            isPushedDown = true;
            sliderImage.rectTransform.anchoredPosition = new Vector3(0, -1);
            foreach (Image i in borders)
            {
                i.rectTransform.anchoredPosition = new Vector3(0, -1);
            }
            pushedDownFrame.gameObject.SetActive(true);
        }
        else if (isPushedDown && !value)
        {
            isPushedDown = false;
            sliderImage.rectTransform.anchoredPosition = new Vector3(0, 0);
            foreach (Image i in borders)
            {
                i.rectTransform.anchoredPosition = new Vector3(0, 0);
            }
            foreach (Image i in frames)
            {
                i.gameObject.SetActive(false);
            }
            SetHighlighted(isLightning); //Edge case where you set lightning while tile is moving. Needed for desert scroll scrap
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
        // If lightning is active, tile should never be unhighlighted
        else if (isHighlighted && !value && !isLightning && !isForcedHighlighted)
        {
            isHighlighted = false;
            foreach (Image i in borders)
            {
                i.gameObject.SetActive(false);
            }
        }
    }

    public void SetForceHighlighted(bool value)
    {
        isForcedHighlighted = value;
        SetHighlighted(value);
    }

    public void SetLightning(bool value, int styleIndex=1)
    {
        if (!isLightning && value)
        {
            highlightedFrame.gameObject.SetActive(false);
            pushedDownFrame.gameObject.SetActive(false);
            Image lightningPushedDown = frames[styleIndex];
            Image lightningHighlight = borders[styleIndex];
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
            Image pushedDown = frames[0];
            Image highlighted = borders[0];
            pushedDownFrame = pushedDown;
            highlightedFrame = highlighted;
            if (isPushedDown) pushedDownFrame.gameObject.SetActive(true);
        }
        isLightning = value;
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
        positionAnimationFinalPosition = to;

        float t = 0;
        while (t < positionAnimationDuration)
        {
            float c = positionAnimationCurve.Evaluate(t / positionAnimationDuration);
            Vector2 pos = Vector2.Lerp(from, to, c);

            rectTransform.anchoredPosition = pos;

            yield return null;
            t += Time.deltaTime;
        }

        rectTransform.anchoredPosition = positionAnimationFinalPosition;
        positionAnimatorCoroutine = null;
    }

    private void AnimatePositionEnd()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = positionAnimationFinalPosition;
        positionAnimatorCoroutine = null;
    }
}
