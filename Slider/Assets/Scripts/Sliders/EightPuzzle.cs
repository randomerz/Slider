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
        AddSlider(2);
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

    public static bool MoveSlider(int x, int y, Direction dir)
    {
        Debug.Log("trying to move slider at " + x + " " + y + " " + dir);
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
            Debug.Log(grid[x + (int)d.x + 1, y + (int)d.y + 1].islandId + " is in the way");
            return false;
        }

        return true;
    }

    public static void AddSlider(int islandId)
    {
        foreach (TileSlider s in grid)
        {
            if (s.islandId == islandId)
            {
                s.SetEmpty(false);
                return;
            }
        }
    }
}
