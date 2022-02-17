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
        [System.Serializable]
        public class ConditionEvent : UnityEvent<Condition>
        {
        }
        public enum ConditionType
        {
            item,
            grid,
            spec,
        }
        public ConditionType type;
        public Collectible.CollectibleData item;
        public string pattern;
        public ConditionEvent checkBool;

        private bool spec = false;
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
            else if (type == ConditionType.grid)
            {
                if (CheckGrid.contains(SGrid.GetGridString(), pattern))
                {
                    Debug.Log("Fishy");
                    return true;
                }
                return false;
            }
            else
            {
                checkBool.Invoke(this);
                return spec;
            }
        }
        public void SetSpec(bool b)
        {
            spec = b;
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
