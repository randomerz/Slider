using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuinsArrow : MonoBehaviour
{
    public Sprite[] arrows; // NNE, NE, ENE, ESE, SE, SSE

    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        SGrid.OnGridMove += UpdateArrowOnStart;
        SGridAnimator.OnSTileMoveEnd += UpdateArrowOnEnd;
    }

    private void OnDestroy()
    {
        SGrid.OnGridMove -= UpdateArrowOnStart;
        SGridAnimator.OnSTileMoveEnd -= UpdateArrowOnEnd;
    }

    private void UpdateArrowOnStart(object sender, SGrid.OnGridMoveArgs e) // SGrid
    {
        bool beforeAssembled = AreRuinsAssembled(SGrid.GetGridString(e.oldGrid));
        bool afterAssembled = AreRuinsAssembled(SGrid.GetGridString(e.grid));
        bool turnOn = beforeAssembled && afterAssembled;

        SetArrowActive(turnOn);
    }

    private void UpdateArrowOnEnd(object sender, SGridAnimator.OnTileMoveArgs e) // SGridAnimator
    {
        SetArrowActive(AreRuinsAssembled(SGrid.GetGridString(SGrid.current.GetGrid())));
    }

    private bool AreRuinsAssembled(string gridString)
    {
        return CheckGrid.contains(gridString, "31..62");
    }

    public void SetArrowActive(bool value)
    {
        // check if arrow should be on or not
        if (value)
        {
            spriteRenderer.enabled = true;
        }
        else
        {
            spriteRenderer.enabled = false;
        }
    }
}
