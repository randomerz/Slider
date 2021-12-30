using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMove
{
    public List<Vector4Int> moves = new List<Vector4Int>(); // move tile at (x, y) to (z, w)

    public Dictionary<Vector2Int, List<int>> borders = new Dictionary<Vector2Int, List<int>>();

    public void GenerateBorders()
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        foreach (Vector4Int m in moves)
        {
            // special case if they are two away and you are sliding
        }
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
        Vector2Int last = points[points.Count];
        moves.Add(new Vector4Int(last.x, last.y, first.x, first.y));
    }
}
