using UnityEngine;

public class MilitarySTile : STile 
{
    public bool hasRightWall;
    public bool hasUpWall;
    public bool hasLeftWall;
    public bool hasDownWall;

    public Transform sliderCollectibleSpawn;
    public Transform newUnitSelectorSpawn;

    private new void Awake() {
        base.Awake();
        // STILE_WIDTH = 13;
    }

    public bool CanMoveFromDirection(Vector2Int dir)
    {
        if (dir == Vector2Int.right)
        {
            return !hasLeftWall;
        }
        else if (dir == Vector2Int.up)
        {
            return !hasDownWall;
        }
        else if (dir == Vector2Int.left)
        {
            return !hasRightWall;
        }
        else if (dir == Vector2Int.down)
        {
            return !hasUpWall;
        }
        else
        {
            Debug.LogError($"CanMoveFromDirection recieved an invalid input. Returning True anyway");
            return true;
        }
    }

    public bool CanMoveToDirection(Vector2Int dir)
    {
        if (dir == Vector2Int.right)
        {
            return !hasRightWall;
        }
        else if (dir == Vector2Int.up)
        {
            return !hasUpWall;
        }
        else if (dir == Vector2Int.left)
        {
            return !hasLeftWall;
        }
        else if (dir == Vector2Int.down)
        {
            return !hasDownWall;
        }
        else
        {
            Debug.LogError($"CanMoveFromDirection recieved an invalid input. Returning True anyway");
            return true;
        }
    }
}