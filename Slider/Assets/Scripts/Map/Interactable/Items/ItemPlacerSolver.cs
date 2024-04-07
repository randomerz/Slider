using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPlacerSolver
{
    //C: Attempts to place the given item within maxDistance of targetPos
    //If forceSameStile is true, then the object will only be place on the same tile the targetpos falls on
    public static bool TryPlaceItem(Vector3 targetPos, Transform itemTransform, int maxDistance, LayerMask blocksSpawnMask, bool forceSameStile)
    {

        Vector3 pos = FindItemPlacePosition(targetPos, maxDistance, blocksSpawnMask, forceSameStile);
        if(pos.x == float.MaxValue)
        {
            return false;
        }
        else
        {
            itemTransform.position = pos;
            itemTransform.transform.parent = SGrid.GetSTileUnderneath(itemTransform, null).transform;
            return true;
        }
    }

    public static Vector3 FindItemPlacePosition(Vector3 targetPos, int maxDistance, LayerMask blocksSpawnMask, bool forceSameStile)
    {

        STile sTile = SGrid.GetSTileUnderneath(targetPos);
        int tries = 0;
        do
        {
            for(int i = 0; i <= 4 * tries; i++ )
            {
                float theta = (2f * Mathf.PI * i) / (4 * Mathf.Max(1, tries));
                Vector3 checkPos = targetPos + tries * new Vector3(Mathf.Cos(theta), Mathf.Sin(theta));
                var cast = Physics2D.OverlapBoxAll(checkPos, Vector2.one, blocksSpawnMask);
                bool valid = true;
                foreach(Collider2D c in cast)
                {
                    if(!c.isTrigger)
                        valid = false;
                }
                if(valid && (!forceSameStile || SGrid.GetSTileUnderneath(checkPos) == sTile))
                {
                    return checkPos;
                }   
            }
            tries++;
        }
        while (tries <= maxDistance);
        return new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    }      
}
