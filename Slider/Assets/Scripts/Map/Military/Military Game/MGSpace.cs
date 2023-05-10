using System.Collections.Generic;
using FMOD;

public class MGSpace
{
    private List<MGEntity> _occupyingEntities;

    public MGSpace()
    {
        _occupyingEntities = new List<MGEntity>();
    }

    public void AddEntity(MGEntity entity)
    {
        _occupyingEntities.Add(entity);
    }
}