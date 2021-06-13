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

    public static void AddSlider(int islandId)
    {
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



    public static void ShuffleBoard()
    {
        _instance.StartCoroutine(ShuffleBoardScene());
    }

    private static IEnumerator ShuffleBoardScene()
    {
        UIFadeIn.FlashWhite();

        yield return new WaitForSeconds(0.5f);

        int[,] puzzle = GetShuffledBoard();
        TileSlider[,] newGrid = new TileSlider[3, 3];
        TileSlider next = null;

        int playerIsland = Player.GetSliderUnderneath();
        Vector3 playerOffset = GetSlider(playerIsland).transform.position - Player.GetPosition();

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                Debug.Log(puzzle[x, y]);
                if (puzzle[x, y] == 0)
                    next = GetSlider(9);
                else
                    next = GetSlider(puzzle[x, y]);

                next.xPos = x - 1;
                next.yPos = y - 1;
                newGrid[x, y] = next;
                next.Awake();
                UIArtifact.SetButtonPos(next.islandId, x - 1, y - 1);
            }
        }

        Player.SetPosition(GetSlider(playerIsland).transform.position + playerOffset);

        grid = newGrid;
    }

    private static int[,] GetShuffledBoard()
    {
        int[] p = { 1, 2, 3, 4, 0, 5, 6, 7, 8 };
        int[,] puzzle = new int[3, 3];

        bool puzzleWorks = false;

        while (!puzzleWorks)
        {
            p = ShuffleArray(p);

            for (int i = 0; i < p.Length; i++)
            {
                puzzle[i % 3, i / 3] = p[i];
            }

            puzzleWorks = CheckInversions.IsSolvable(puzzle);
        }

        return puzzle;
    }

    private static int[] ShuffleArray(int[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int k = Random.Range(0, i + 1);
            int temp = arr[i];
            arr[i] = arr[k];
            arr[k] = temp;
        }
        return arr;
    }
}
