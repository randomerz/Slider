using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//L: A representation of a move made on the artifact, as well as the borders around the move that prevent the player from clipping.
public class SMove
{
    public List<Vector4Int> moves = new List<Vector4Int>(); // move tile at (x, y) to (z, w)

    //L: Every (x, y) position that is touched by moves
    public HashSet<Vector2Int> positions = new HashSet<Vector2Int>();   
    //L: key - (x, y) position of a tile
    //L: Value - A list of values taken from 0, 1, 2, 3 (maximum of 4 values) for each tile side denoting which borderse are on.
    public Dictionary<Vector2Int, List<int>> borders = new Dictionary<Vector2Int, List<int>>();

    public Dictionary<Vector2Int, List<int>> GenerateBorders()
    {
        positions.Clear();
        borders.Clear();

        foreach (Vector4Int m in moves)
        {
            // for the cases where its swapping more than two
            for (int x = Mathf.Min(m.x, m.z); x <= Mathf.Max(m.x, m.z); x++)
            {
                for (int y = Mathf.Min(m.y, m.w); y <= Mathf.Max(m.y, m.w); y++)
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }
        }

        foreach (Vector2Int p in positions)
        {
            AddBorder(p, 0, p + Vector2Int.right);
            AddBorder(p, 1, p + Vector2Int.up);
            AddBorder(p, 2, p + Vector2Int.left);
            AddBorder(p, 3, p + Vector2Int.down);
        }

        return borders;
    }

    //L: side - 0, 1, 2, 3 corresponding to right, up, left, and down sides of a tile
    private void AddBorder(Vector2Int pos1, int side, Vector2Int pos2)
    {
        if (!borders.ContainsKey(pos1))
            borders.Add(pos1, new List<int>());
        if (!borders.ContainsKey(pos2))
             borders.Add(pos2, new List<int>());

        // toggle sides
        /*
         * L: We want to toggle the sides as opposed to just adding them because if a border is hit twice, it means that it is in the path of the move and is actually not a border.
         * Ex: If you are moving from (0, 1) to (1,1). 
         * First process (0,1) after which The top, left, bottom, and right sides of (0,1) will be added to the list. The left side of (1,1) is added when (1,1) is pos2.
         * Next, while (1,1) is being processed, since the left side has already been added, it will be removed, while the top, right, and bottom borders are added.
         *  ___ ___                              _______                       
         * |   |   | - without toggling         |       | - with toggling
         * |___|___|                            |_______|
         */
        if (borders[pos1].Contains(side))
            borders[pos1].Remove(side);
        else
            borders[pos1].Add(side);
        //L: The sides are with respect to pos1, so the opposite side is added for pos2 (think geometrically with 2 tiles that share a side)
        if (borders[pos2].Contains((side + 2) % 4))
            borders[pos2].Remove((side + 2) % 4);
        else
            borders[pos2].Add((side + 2) % 4);
    }
}

public class Vector4Int
{
    public int x; // x, y is the original location
    public int y;
    public int z; // z, w is the target location
    public int w;

    public Vector4Int(int x, int y, int z, int w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
}


//L: Swapping includes moving a tile to an empty spot!
public class SMoveSwap : SMove
{
    public SMoveSwap(int x1, int y1, int x2, int y2)
    {
        moves.Add(new Vector4Int(x1, y1, x2, y2));
        moves.Add(new Vector4Int(x2, y2, x1, y1));
    }

    public Vector4Int GetSwapAsVector()
    {
        return moves[0];
    }
}

//L: Used primarily in the "Ocean" area for rotating tiles around
public class SMoveRotate : SMove
{
    public SMoveRotate(List<Vector2Int> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            moves.Add(new Vector4Int(points[i].x, points[i].y, points[(i + 1) % points.Count].x, points[(i + 1) % points.Count].y));
        }

    }
}
