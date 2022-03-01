using System.Collections.Generic;
using UnityEngine;

public class LocalizedNodeType
{
    private Vector2Int pos;

    public Vector2Int Position
    {
        get
        {
            return pos;
        }
    }

    public LocalizedNodeType(Vector2Int pos)
    {
        this.pos = pos;
    }

    public int GetCostTo(LocalizedNodeType other)
    {
        return Mathf.RoundToInt(10 * Vector2Int.Distance(pos, other.pos));
    }
}

