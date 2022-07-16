using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class STileCrossing
{
    public STile from;
    public STile to;

    [FormerlySerializedAs("fromPos")]
    public Vector2Int fromPosIfNull;
    [FormerlySerializedAs("toPos")]
    public Vector2Int toPosIfNull;

    public Vector2Int[] dirs;

    public bool CrossingIsValid()
    {
        Vector2Int fromPos = from ? new Vector2Int(from.x, from.y) : fromPosIfNull;
        Vector2Int toPos = to ? new Vector2Int(to.x, to.y) : toPosIfNull;

        foreach (Vector2Int dir in dirs)
        {
            if (DirValid(dir, fromPos, toPos))
            {
                return true;
            }
        }

        return false;
    }

    private bool DirValid(Vector2Int dir, Vector2Int from, Vector2Int to)
    {
        return (to - from).Equals(dir);
    }
}
