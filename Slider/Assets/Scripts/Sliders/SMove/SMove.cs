using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMove
{
    public List<Vector4Int> moves = new List<Vector4Int>(); // move tile at (x, y) to (z, w)

    public HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
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

    private void AddBorder(Vector2Int pos1, int side, Vector2Int pos2)
    {
        if (!borders.ContainsKey(pos1))
            borders.Add(pos1, new List<int>());
        if (!borders.ContainsKey(pos2))
            borders.Add(pos2, new List<int>());

        // toggle sides
        if (borders[pos1].Contains(side))
            borders[pos1].Remove(side);
        else
            borders[pos1].Add(side);

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



public class SMoveSwap : SMove
{
    public SMoveSwap(int x1, int y1, int x2, int y2)
    {
        moves.Add(new Vector4Int(x1, y1, x2, y2));
        moves.Add(new Vector4Int(x2, y2, x1, y1));
    }
}


public class SMoveRotate : SMove
{
    public SMoveRotate(List<Vector2Int> points)
    {
        for (int i = 0; i < points.Count - 1; i++)
        {
            moves.Add(new Vector4Int(points[i].x, points[i].y, points[i + 1].x, points[i + 1].y));
        }
        Vector2Int first = points[0];
        Vector2Int last = points[points.Count - 1];
        moves.Add(new Vector4Int(last.x, last.y, first.x, first.y));
    }
}
