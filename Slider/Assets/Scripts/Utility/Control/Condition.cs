using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// ConditionalsEditor

[System.Serializable]
public class Condition
{
    public enum ConditionType
    {
        item, // DC this should be called collectible but i don't want to rename it bc it will break everything
        grid,
        gridStationary, //L: Forces the grid to be stationary before it can evaluate the condition to true.
        spec,
        playerCarryingItem,
        flag,
        playerCarryingSpecItem, // unused?
        noGrid, //C: returns true if grid does *not* contain the pattern
        noFlag,
    }
    [System.Serializable]
    public class ConditionEvent : UnityEvent<Condition>
    {
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
        switch (type)
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
            case ConditionType.noGrid:
                if (!CheckGrid.contains(SGrid.GetGridString(), pattern))
                {
                    return true;
                }
                return false;
            case ConditionType.gridStationary:
                if (CheckGrid.contains(SGrid.GetGridString(), pattern))
                {
                    foreach (int i in stationaryTiles)
                    {
                        if (SGrid.Current.GetStile(i).IsMoving())
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
                if (SaveSystem.IsCurrentProfileNull())
                    return false;
                return SaveSystem.Current.GetBool(flagName);
            case ConditionType.noFlag:
                if (SaveSystem.IsCurrentProfileNull())
                    return false;
                return !SaveSystem.Current.GetBool(flagName);
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
