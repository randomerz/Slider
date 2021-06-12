using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EightPuzzle : MonoBehaviour
{
    public float slideCooldown = 1;
    private float timeTillCanSlide;
    private bool canSlide;

    private static Slider[,] grid = new Slider[3, 3];
    private static EightPuzzle _instance;

    // References
    public Slider[] sliders;

    private void Awake()
    {
        _instance = this;

        if (sliders.Length != 9)
        {
            Debug.LogWarning("Only " + sliders.Length + " found! Expected 9.");
            return;
        }
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                grid[r, c] = sliders[3 * c + r];
            }
        }
    }

    public static EightPuzzle GetInstance()
    {
        return _instance;
    }

    public static bool MoveSlider(int x, int y, Direction dir)
    {
        if (!CanMoveSlider(x, y, dir))
        {
            return false;
        }

        // Swap sliders
        Vector2 d = DirectionUtil.D2V(dir);
        int x2 = x + (int)d.x;
        int y2 = y + (int)d.y;
        grid[x, y].SetPosition(x2, y2);
        grid[x2, y2].SetPosition(x, y);

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
        if (x + d.x < 0 || x + d.x > 3 || y + d.y < 0 || y + d.y > 3)
        {
            return false;
        }

        // tile in direction
        if (!grid[x + (int)d.x, y + (int)d.y].isEmpty)
        {
            return false;
        }

        return true;
    }
}
