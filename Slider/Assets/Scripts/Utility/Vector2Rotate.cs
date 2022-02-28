using UnityEngine;


public class Vector2Rotate
{
    //L: Rotation is CW (-radians for CCW)
    public static Vector2 Rotate(Vector2 v, float radians)
    {
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        float oldX = v.x;
        float oldY = v.y;
        v.x = cos * oldX + sin * oldY;
        v.y = -sin * oldX + cos * oldY;
        return v;
    }
}

