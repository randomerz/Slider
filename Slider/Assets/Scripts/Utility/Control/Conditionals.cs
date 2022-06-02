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
            item, // DC this should be called collectible but i don't want to rename it bc it will break everything
            grid,
            gridStationary, //L: Forces the grid to be stationary before it can evaluate the condition to true.
            spec,
            playerCarryingItem,
            flag,
        }
        public ConditionType type;

        // collectible
        public Collectible.CollectibleData item;

        //grid
        public string pattern;
        public List<int> stationaryTiles;

        //spec
        public ConditionEvent checkBool;

        // player item
        public string playerItemName;

        //flag
        public string flagName;

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
                case ConditionType.playerCarryingItem:
                    if (!Player.GetPlayerAction().HasItem())
                    {
                        return false;
                    }
                    if (!Player.GetPlayerAction().pickedItem.itemName.Equals(playerItemName))
                    {
                        return false;
                    }
                    return true;
                case ConditionType.flag:
                    return SaveSystem.Current.GetBool(flagName);
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
    [Header("Base Condition Events (DO NOT USE FOR DIALOGUE)")]
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
