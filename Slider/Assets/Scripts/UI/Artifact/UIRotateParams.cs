using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIRotateParams : MonoBehaviour
{
    [Header("Params to set")]
    public int bottomLeftX;
    public int bottomLeftY;

    private Sprite unlitCWArrow;
    private Sprite unlitCCWArrow;
    public Sprite litCWArrow;
    public Sprite litCCWArrow;


    private bool isCCW;
    private bool isHoveredOver;


    private RectTransform canvasRectTransform;
    private RectTransform myRectTransform;
    private Vector2 uiOffset;

    [Header("References")]
    public Image cwArrowSprite;
    public Image ccwArrowSprite;
    public OceanArtifact artifact;
    public Canvas canvas;

    private void Start() 
    {
        canvasRectTransform = canvas.GetComponent<RectTransform>();
        myRectTransform = GetComponent<RectTransform>();
        uiOffset = new Vector2((float)canvasRectTransform.sizeDelta.x / 2f, 
                            (float)canvasRectTransform.sizeDelta.y / 2f);

        unlitCWArrow  = cwArrowSprite.sprite;
        unlitCCWArrow = ccwArrowSprite.sprite;
    }

    private void Update() 
    {
        if (isHoveredOver) {
            CalcMousePos();
        }
    }

    public void OnHover() 
    {
        // SelectArrow() the closest arrow

        isHoveredOver = true;
        CalcMousePos();
    }

    private void CalcMousePos() 
    {
        // get mouse position
        Vector2 viewportPos = Camera.main.ScreenToViewportPoint(Mouse.current.position.ReadValue());
        
        Vector2 proportionalPos = new Vector2(viewportPos.x * canvasRectTransform.sizeDelta.x, 
                                              viewportPos.y * canvasRectTransform.sizeDelta.y);
         
        Vector2 offsetPos = proportionalPos - uiOffset - myRectTransform.anchoredPosition;

        // if it's the bottom right area, then ccw is true
        SelectArrow(offsetPos.x > offsetPos.y);
        
        // Debug.Log("hovering, is ccw: " + (offsetPos.x > offsetPos.y));
    }

    public void OnHoverExit() 
    {
        cwArrowSprite.sprite  = unlitCWArrow;
        ccwArrowSprite.sprite = unlitCCWArrow;

        isHoveredOver = false;
    }

    public void SelectArrow(bool ccw) 
    {
        // Debug.Log("selected ccw: " + ccw);
        isCCW = ccw; 

        cwArrowSprite.sprite  = ccw ? unlitCWArrow : litCWArrow;
        ccwArrowSprite.sprite = ccw ? litCCWArrow : unlitCCWArrow;

        // Debug.Log(isCCW);
    }

    public void OnClick()
    {
        // OnHover();

        artifact.RotateTiles(bottomLeftX, bottomLeftY, isCCW);

    }
}
