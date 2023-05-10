
using UnityEngine;

public class MGEvent
{
}

public class MGSpawnEvent : MGEvent
{
    public MGEntity entitySpawned;
    public Vector2Int pos;

    public MGSpawnEvent(MGEntity entity, Vector2Int pos)
    {
        this.entitySpawned = entity;
        this.pos = pos;
    }
}