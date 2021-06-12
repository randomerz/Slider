using UnityEngine;

public enum Direction
{
    RIGHT,
    UP,
    LEFT,
    DOWN
}

public class DirectionUtil : MonoBehaviour
{
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
}