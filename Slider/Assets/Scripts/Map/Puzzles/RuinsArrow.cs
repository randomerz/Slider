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

        SetArrowActive(turnOn, e.oldGrid); // probably doesnt matter which grid
    }

    private void UpdateArrowOnEnd(object sender, SGridAnimator.OnTileMoveArgs e) // SGridAnimator
    {
        SetArrowActive(
            AreRuinsAssembled(SGrid.GetGridString(SGrid.current.GetGrid())), 
            SGrid.current.GetGrid()
        );
    }

    private bool AreRuinsAssembled(string gridString)
    {
        return CheckGrid.contains(gridString, "31..62");
    }

    public void SetArrowActive(bool value, STile[,] grid)
    {
        // check if arrow should be on or not
        if (value)
        {
            spriteRenderer.enabled = true;
            UpdateArrowDirection(grid);

            SGrid.current.ActivateSliderCollectible(7);
        }
        else
        {
            spriteRenderer.enabled = false;
        }
    }

    private void UpdateArrowDirection(STile[,] grid)
    {
        Vector2Int t2 = Vector2Int.zero;
        Vector2Int t5 = Vector2Int.zero;

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                if (grid[x, y].islandId == 2)
                    t2 = new Vector2Int(x, y);
                else if (grid[x, y].islandId == 5)
                    t5 = new Vector2Int(x, y);
            }
        }

        Vector2Int dif = t5 - t2;
        
        if (dif.y == 2)
        {
            // NNE
            if (dif.x == 0 || dif.x == -1)
            {
                spriteRenderer.sprite = arrows[0];
            }
            // NE
            else if (dif.x == 1 || dif.x == -2)
            {
                spriteRenderer.sprite = arrows[1];
            }
        }
        else if (dif.y == 1)
        {
            // ENE
            spriteRenderer.sprite = arrows[2];
        }
        else if (dif.y == 0)
        {
            // ESE
            spriteRenderer.sprite = arrows[3];
        }
        else if (dif.y == -1)
        {
            // SE
            if (dif.x == 1 || dif.x == -2)
            {
                spriteRenderer.sprite = arrows[4];
            }
            // SSE
            else if (dif.x == 0 || dif.x == -1)
            {
                spriteRenderer.sprite = arrows[5];
            }
        }

        spriteRenderer.flipX = (dif.x < 0);
    }
}
