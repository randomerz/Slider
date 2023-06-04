using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// Used in the ocean area
public class UIRotateParams : MonoBehaviour
{
    [Header("Params to set")]
    public int bottomLeftX;
    public int bottomLeftY;

    // private Sprite unlitCWArrow;
    // private Sprite unlitCCWArrow;
    // public Sprite litCWArrow;
    // public Sprite litCCWArrow;

    private bool isCCW;
    private bool isHoveredOver;


    private RectTransform canvasRectTransform;
    private RectTransform myRectTransform;
    private Vector2 uiOffset;

    [Header("References")]
    public Animator animator;
    public OceanArtifact artifact;
    public Canvas canvas;

    private void Start() 
    {
        canvasRectTransform = canvas.GetComponent<RectTransform>();
        myRectTransform = GetComponent<RectTransform>();
        uiOffset = new Vector2((float)canvasRectTransform.sizeDelta.x / 2f, 
                            (float)canvasRectTransform.sizeDelta.y / 2f);

        // unlitCWArrow  = cwArrowSprite.sprite;
        // unlitCCWArrow = ccwArrowSprite.sprite;
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
        // Screen Space Pos -> Viewport Pos
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 normalizedPos = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);

        //Viewport point -> Canvas Space Pt.
        Vector2 proportionalPos = new Vector2(normalizedPos.x * canvasRectTransform.sizeDelta.x,
                                              normalizedPos.y * canvasRectTransform.sizeDelta.y);
         
        //The position relative to the center of the rotation gizmo thingy.
        Vector2 offsetPos = proportionalPos - uiOffset - myRectTransform.anchoredPosition;

        // if it's the bottom right area, then ccw is true
        SelectArrow(offsetPos.x > offsetPos.y);

        //Debug.Log($"hovering over {gameObject.name}, is ccw: {(offsetPos.x > offsetPos.y)}");
        //Debug.Log($"hovering over {gameObject.name}, offsetPos: {offsetPos}");
        //Debug.Log($"hovering over {gameObject.name}, proportionalPos: {proportionalPos}");
        //Debug.Log($"Camera Dimensions: {Camera.main.pixelWidth}, {Camera.main.pixelHeight}");
        //Debug.Log($"hovering over {gameObject.name}, normalizedPos: {normalizedPos}");
        //Debug.Log($"hovering over {gameObject.name}, mcpv: {Mouse.current.position.ReadValue()}");
        //Debug.Log($"Size Delta: {canvasRectTransform.sizeDelta}");
        //Debug.Log($"uiOffset: {uiOffset}");
        //Debug.Log($"Anchored Position: {myRectTransform.anchoredPosition}");
    }

    public void OnHoverExit() 
    {
        // cwArrowSprite.sprite  = unlitCWArrow;
        // ccwArrowSprite.sprite = unlitCCWArrow;
        animator.SetBool("highlightCW", false);
        animator.SetBool("highlightCCW", false);

        isHoveredOver = false;
    }

    public void SelectArrow(bool ccw) 
    {
        // Debug.Log("selected ccw: " + ccw);
        isCCW = ccw; 

        animator.SetBool("highlightCW", !ccw);
        animator.SetBool("highlightCCW", ccw);

        // Debug.Log(isCCW);
    }

    public void RotateArrow(bool ccw)
    {
        if (ccw)
        {
            animator.SetTrigger("rotateCCW");
        }
        else
        {
            animator.SetTrigger("rotateCW");
        }
    }
    [SerializeField] private OceanControllerSupportButtonsHolder oceanControllerSupportButtonsHolder;
    public void OnClick()
    {
        // OnHover();

        // artifact.AddQueue(bottomLeftX, bottomLeftY, isCCW);
        artifact.RotateTiles(bottomLeftX, bottomLeftY, isCCW);

    }
}
