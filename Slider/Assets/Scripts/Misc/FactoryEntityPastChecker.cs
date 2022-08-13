using UnityEngine;

public class FactoryEntityPastChecker
{
    //Yes I'm Serious
    public static bool IsInPast(GameObject entity)
    {
        return entity.transform.position.y < -50f;
    }
}

