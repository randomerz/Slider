using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuinsArrow : MonoBehaviour
{
    [SerializeField] private Sprite[] arrows; // NNE, NE, ENE, ESE, SE, SSE
    [SerializeField] private RuinsArrowRod rod1;
    [SerializeField] private RuinsArrowRod rod2;
    [SerializeField] private RuinsArrowRod rod3;
    [SerializeField] private RuinsArrowRod rod6;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem arrowParticles;
    [SerializeField] private FlashWhite arrowFlash;
    [SerializeField] private RuinsMapIcons mapIcons;

    private bool anyRodsOn;

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

        UpdateMap(SGrid.GetGridString(e.grid));
    }

    private void UpdateArrowOnEnd(object sender, SGridAnimator.OnTileMoveArgs e) // SGridAnimator
    {
        SetArrowActive(
            AreRuinsAssembled(SGrid.GetGridString(SGrid.current.GetGrid())), 
            SGrid.current.GetGrid()
        );

        UpdateRods(SGrid.GetGridString(SGrid.current.GetGrid()));
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
            arrowParticles.Play();
            UpdateArrowDirection(grid);

            SGrid.current.ActivateSliderCollectible(7);
        }
        else
        {
            spriteRenderer.enabled = false;
            arrowParticles.Stop();
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
        Sprite oldSprite = spriteRenderer.sprite;
        
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

        bool oldFlipX = spriteRenderer.flipX;
        spriteRenderer.flipX = (dif.x < 0);

        if (oldSprite != spriteRenderer.sprite || oldFlipX != spriteRenderer.flipX)
        {
            // doesnt look good :/ but if we wanna do something else when it transitions
            //arrowFlash.Flash(1);
        }
    }

    private void UpdateMap(string gridString)
    {
        mapIcons.ResetIcons();

        if (CheckGrid.contains(gridString, "31"))
        {
            mapIcons.SetMapIcon(1, true);
            mapIcons.SetMapIcon(3, true);
        }
        if (CheckGrid.contains(gridString, "62"))
        {
            mapIcons.SetMapIcon(2, true);
            mapIcons.SetMapIcon(6, true);
        }
        if (CheckGrid.contains(gridString, "3...6"))
        {
            mapIcons.SetMapIcon(3, true);
            mapIcons.SetMapIcon(6, true);
        }
        if (CheckGrid.contains(gridString, "1...2"))
        {
            mapIcons.SetMapIcon(1, true);
            mapIcons.SetMapIcon(2, true);
        }
    }

    private void UpdateRods(string gridString)
    {
        ResetRods();
        mapIcons.ResetIcons();

        if (CheckGrid.contains(gridString, "31"))
        {
            rod1.SetRod(true);
            rod3.SetRod(true);
            mapIcons.SetMapIcon(1, true);
            mapIcons.SetMapIcon(3, true);
            anyRodsOn = true;
        }
        if (CheckGrid.contains(gridString, "62"))
        {
            rod2.SetRod(true);
            rod6.SetRod(true);
            mapIcons.SetMapIcon(2, true);
            mapIcons.SetMapIcon(6, true);
            anyRodsOn = true;
        }
        if (CheckGrid.contains(gridString, "3...6"))
        {
            rod3.SetRod(true);
            rod6.SetRod(true);
            mapIcons.SetMapIcon(3, true);
            mapIcons.SetMapIcon(6, true);
            anyRodsOn = true;
        }
        if (CheckGrid.contains(gridString, "1...2"))
        {
            rod1.SetRod(true);
            rod2.SetRod(true);
            mapIcons.SetMapIcon(1, true);
            mapIcons.SetMapIcon(2, true);
            anyRodsOn = true;
        }
    }

    private void ResetRods()
    {
        rod1.SetRod(false);
        rod2.SetRod(false);
        rod3.SetRod(false);
        rod6.SetRod(false);
        anyRodsOn = false;
    }

    public void CheckActiveRods(Conditionals.Condition c)
    {
        c.SetSpec(anyRodsOn);
    }
}
