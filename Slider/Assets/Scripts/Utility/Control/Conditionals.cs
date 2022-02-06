using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Conditionals
{
    [System.Serializable]
    public class Condition
    {
        public enum ConditionType
        {
            item,
            grid,
        }
        public ConditionType type;
        public Collectible.CollectibleData item;
        public string pattern;

        public bool CheckCondition()
        {
            if (type == ConditionType.item)
            {
                if (PlayerInventory.Contains(item.name, item.area))
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
    public UnityEvent onSuccess;
    public UnityEvent onFail;
    public List<Condition> conditions;
    public bool CheckConditions() 
    {
        foreach (Condition cond in conditions) 
        {
            if (!cond.CheckCondition())
            {
                onFail?.Invoke();
                return false;
            }
        }
        onSuccess?.Invoke();
        return true;
    }
}
