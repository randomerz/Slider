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
            gridStationary, //L: Forces the grid to be stationary before it can evaluate the condition to true.
            spec,
        }
        public ConditionType type;

        //item
        public Collectible.CollectibleData item;

        //grid
        public string pattern;
        public List<int> stationaryTiles;

        //spec
        public ConditionEvent checkBool;

        private bool spec = false;
        public bool CheckCondition()
        {
            switch(type)
            {
                case ConditionType.item:
                    if (PlayerInventory.Contains(item.name, item.area))
                    {
                        return true;
                    }
                    return false;
                case ConditionType.grid:
                    if (CheckGrid.contains(SGrid.GetGridString(), pattern))
                    {
                        return true;
                    }
                    return false;
                case ConditionType.gridStationary:
                    if (CheckGrid.contains(SGrid.GetGridString(), pattern))
                    {
                        foreach (int i in stationaryTiles)
                        {
                            if (SGrid.current.GetStile(i).GetMovingDirection() != Vector2.zero)
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    return false;
                case ConditionType.spec:
                default:
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
