using System;
using System.Collections.Generic;
using UnityEngine;

public class MGSpace
{
    private Dictionary<MGUnits.Unit, int> _units;   //EntityData, Qty

    public delegate void _OnSupplyDropSpawn();
    public event _OnSupplyDropSpawn OnSupplyDropSpawn;

    public delegate void _OnUnitsChanged(MGUnits.Unit unit, int quantity);
    public event _OnUnitsChanged OnUnitsChanged;

    public MGSpace()
    {
        _units = new();
    }

    public void AddUnits(MGUnits.Unit unit, int quantity)
    {
        if (quantity <= 0)
        {
            return;
        }


        if (_units.ContainsKey(unit))
        {
            _units[unit] += quantity;
        }
        else
        {
            _units[unit] = quantity;
        }

        OnUnitsChanged?.Invoke(unit, _units[unit]);
    }

    public void SpawnSupplyDrop()
    {
        Debug.Log("Supply Drop!");
        OnSupplyDropSpawn?.Invoke();
    }

    public void PrintUnitData()
    {
        String unitDataStr = "Units: \n";

        foreach (MGUnits.Unit unit in _units.Keys)
        {
            unitDataStr += $"{unit.side}-{unit.job}: {_units[unit]}\n";
        }

        Debug.Log(unitDataStr);
    }
}