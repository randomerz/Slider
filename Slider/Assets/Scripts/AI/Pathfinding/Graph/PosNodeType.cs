using System.Collections.Generic;
using UnityEngine;

public class PosNodeType
{
    private Vector2Int pos;

    public Vector2Int Position
    {
        get
        {
            return pos;
        }
    }

    public PosNodeType(Vector2Int pos)
    {
        this.pos = pos;
    }

    public int GetCostTo(PosNodeType other)
    {
        return Mathf.Abs(Mathf.RoundToInt(10 * Vector2Int.Distance(pos, other.pos)));
    }

    //make sure types with same position are equal
    public override int GetHashCode()
    {
        return pos.GetHashCode();
    }
}

