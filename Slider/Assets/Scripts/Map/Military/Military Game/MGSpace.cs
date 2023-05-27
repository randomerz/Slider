using System;
using System.Collections.Generic;
using UnityEngine;

public class MGSpace
{
    private int x, y;

    public delegate void _OnSupplyDropSpawn();
    public event _OnSupplyDropSpawn OnSupplyDropSpawn;

    public MGSpace(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2Int GetPosition()
    {
        return new Vector2Int(x, y);
    }

    //public void AddUnits(MGUnitData.Data unit, int quantity)
    //{
    //    if (quantity <= 0)
    //    {
    //        return;
    //    }


    //    if (_units.ContainsKey(unit))
    //    {
    //        _units[unit] += quantity;
    //    }
    //    else
    //    {
    //        _units[unit] = quantity;
    //    }

    //    OnUnitsChanged?.Invoke(unit, _units[unit]);
    //}

    public void SpawnSupplyDrop()
    {
        Debug.Log("Supply Drop!");
        OnSupplyDropSpawn?.Invoke();
    }

    //public void PrintUnitData()
    //{
    //    string unitDataStr = "Units: \n";

    //    foreach (MGUnitData.Data unit in _units.Keys)
    //    {
    //        unitDataStr += $"{unit.side}-{unit.job}: {_units[unit]}\n";
    //    }

    //    Debug.Log(unitDataStr);
    //}
}