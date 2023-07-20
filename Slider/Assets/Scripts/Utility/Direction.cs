using System.Linq;
using UnityEngine;

public enum Direction
{
    RIGHT,
    UP,
    LEFT,
    DOWN
}

public class DirectionUtil
{
    public static Vector2Int[] Cardinal4 =
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

    public static Vector2Int[] Diagonal4 =
    {
        Vector2Int.up + Vector2Int.left,
        Vector2Int.up + Vector2Int.right,
        Vector2Int.down + Vector2Int.left,
        Vector2Int.down + Vector2Int.right
    };

    public static Vector2 D2V(Direction direction)
    {
        switch (direction)
        {
            case Direction.RIGHT:
                return Vector2.right;
            case Direction.UP:
                return Vector2.up;
            case Direction.LEFT:
                return Vector2.left;
            case Direction.DOWN:
                return Vector2.down;
        }
        return Vector2.zero;
    }

    public static Vector2Int D2VInt(Direction direction)
    {
        switch (direction)
        {
            case Direction.RIGHT:
                return Vector2Int.right;
            case Direction.UP:
                return Vector2Int.up;
            case Direction.LEFT:
                return Vector2Int.left;
            case Direction.DOWN:
                return Vector2Int.down;
            default:
                return Vector2Int.zero;
        }
    }

    public static Direction V2D(Vector2 direction)
    {
        if (direction.x > 0)
            return Direction.RIGHT;
        else if (direction.y > 0)
            return Direction.UP;
        else if (direction.x < 0)
            return Direction.LEFT;
        else if (direction.y < 0)
            return Direction.DOWN;
        return 0;
    }
}