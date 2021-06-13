using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EightPuzzle : MonoBehaviour
{
    public float slideCooldown = 1;
    private float timeTillCanSlide;
    private bool canSlide = true;

    private static TileSlider[,] grid = new TileSlider[3, 3];
    private static EightPuzzle _instance;

    // References
    public TileSlider[] sliders;

    private void Awake()
    {
        _instance = this;

        if (sliders.Length != 9)
        {
            Debug.LogWarning("Only " + sliders.Length + " found! Expected 9.");
            return;
        }
        foreach (TileSlider s in sliders)
        {
            grid[s.xPos + 1, s.yPos + 1] = s;
        }


        // Swap tiles 8 & 9 so its solvable
        //int x1 = -1;
        //int y1 = 0;
        //int x2 = 0;
        //int y2 = 0;
        //grid[x1 + 1, y1 + 1].SetPositionRaw(x2, y2);
        //grid[x2 + 1, y2 + 1].SetPositionRaw(x1, y1);

        //TileSlider temp = grid[x1 + 1, y1 + 1];
        //grid[x1 + 1, y1 + 1] = grid[x2 + 1, y2 + 1];
        //grid[x2 + 1, y2 + 1] = temp;
    }

    public static EightPuzzle GetInstance()
    {
        return _instance;
    }

    public static bool GetCanSlide()
    {
        return _instance.canSlide;
    }

    public static TileSlider[,] GetGrid()
    {
        return grid;
    }

    public static TileSlider GetSlider(int islandId)
    {
        foreach (TileSlider s in _instance.sliders)
        {
            if (s.islandId == islandId)
            {
                return s;
            }
        }
        return null;
    }

    public static bool MoveSlider(int x, int y, Direction dir)
    {
        //Debug.Log("trying to move slider at " + x + " " + y + " " + dir);
        if (!CanMoveSlider(x, y, dir))
        {
            return false;
        }

        //Debug.Log("moving!");
        // Swap sliders
        Vector2 d = DirectionUtil.D2V(dir);
        int x2 = x + (int)d.x;
        int y2 = y + (int)d.y;
        grid[x + 1, y + 1].SetPosition(x2, y2);
        grid[x2 + 1, y2 + 1].SetPosition(x, y);

        TileSlider temp = grid[x + 1, y + 1];
        grid[x + 1, y + 1] = grid[x2 + 1, y2 + 1];
        grid[x2 + 1, y2 + 1] = temp;

        if (NPCManager.CheckQRCode())
        {
            ItemManager.ActivateNextItem();
        }

        if (NPCManager.CheckFinalPlacements())
        {
            ItemManager.ActivateNextItem();
        }
        return true;
    }

    private static bool CanMoveSlider(int x, int y, Direction dir)
    {
        if (!_instance.canSlide)
        {
            return false;
        }

        Vector2 d = DirectionUtil.D2V(dir);

        // out of bounds
        if (x + d.x < -1 || x + d.x > 1 || y + d.y < -1 || y + d.y > 1)
        {
            return false;
        }

        // tile in direction
        if (!grid[x + (int)d.x + 1, y + (int)d.y + 1].isEmpty)
        {
            //Debug.Log(grid[x + (int)d.x + 1, y + (int)d.y + 1].islandId + " is in the way");
            return false;
        }

        return true;
    }

    private static void Swap89()
    {
        TileSlider s1 = null, s2 = null;
        foreach (TileSlider s in _instance.sliders)
        {
            if (s.islandId == 8)
            {
                s1 = s;
            }
            if (s.islandId == 9)
            {
                s2 = s;
            }
        }

        int x1 = s1.xPos;
        int y1 = s1.yPos;
        int x2 = s2.xPos;
        int y2 = s2.yPos;
        grid[x1 + 1, y1 + 1].SetPosition(x2, y2);
        grid[x2 + 1, y2 + 1].SetPosition(x1, y1);

        TileSlider temp = grid[x1 + 1, y1 + 1];
        grid[x1 + 1, y1 + 1] = grid[x2 + 1, y2 + 1];
        grid[x2 + 1, y2 + 1] = temp;
    }

    public static void AddSlider(int islandId)
    {
        // before putting 8 in, make sure to put it in the correct spot so puzzle is solvable
        if (islandId == 8)
        {
            int[,] puzzle = new int[3, 3];
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (grid[x, y].islandId == 9)
                        puzzle[x, y] = 0;
                    else
                        puzzle[x, y] = grid[x, y].islandId;
                }
            }

            if (CheckInversions.IsSolvable(puzzle))
                Debug.Log("Solvable");
            else
            {
                // swap 8 and 9
                Swap89();
                UIArtifact.Swap89();
            }
        }

        foreach (TileSlider s in grid)
        {
            if (s.islandId == islandId)
            {
                s.SetEmpty(false);
                UIArtifact.AddButton(islandId);
                return;
            }
        }
    }
}
