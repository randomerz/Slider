using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

[System.Serializable]
public class Condition
{
    public enum ConditionType
    {
        item,
        grid,
    }
    public ConditionType type;
    public Collectible item;
    public string pattern;

    public bool CheckCondition()
    {
        if (type == ConditionType.item)
        {
            if (PlayerInventory.Contains(item.GetName()))
            {
                return true;
            }
            return false;
        } 
        else
        {
            if (CheckGrid.contains(SGrid.GetGridString(), pattern))
            {
                return true;
            }
            return false;
        }
    }
}
