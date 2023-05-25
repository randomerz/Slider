using System;
using System.Collections.Generic;
using UnityEngine;

public class MGSpace
{
    private Dictionary<MGUnits.Job, int> _alliedUnits;   //EntityData, Qty
    private Dictionary<MGUnits.Job, int> _enemyUnits;   //EntityData, Qty

    public delegate void _OnSupplyDropSpawn();
    public event _OnSupplyDropSpawn OnSupplyDropSpawn;

    public delegate void _OnUnitsChanged(MGUnits.Unit unit, int quantity);
    public event _OnUnitsChanged OnUnitsChanged;

    public MGSpace()
    {
        _alliedUnits = new();
        _enemyUnits = new();
    }

    public void AddUnits(MGUnits.Unit unit, int quantity)
    {
        if (quantity <= 0)
        {
            return;
        }

        Dictionary<MGUnits.Job, int> unitQuantities;
        switch (unit.side)
        {
            case MGUnits.Allegiance.Ally:
                unitQuantities = _alliedUnits;
                break;
            case MGUnits.Allegiance.Enemy:
            default:
                unitQuantities = _enemyUnits;
                break;
        }

        if (unitQuantities.ContainsKey(unit.job))
        {
            unitQuantities[unit.job] += quantity;
        }
        else
        {
            unitQuantities[unit.job] = quantity;
        }

        OnUnitsChanged?.Invoke(unit, unitQuantities[unit.job]);
    }

    public void SpawnSupplyDrop()
    {
        Debug.Log("Supply Drop!");
        OnSupplyDropSpawn?.Invoke();
    }

    public void PrintUnitData()
    {
        String allies = "Allied Units: \n";

        foreach (MGUnits.Job job in _alliedUnits.Keys)
        {
            allies += $"{job}: {_alliedUnits[job]}\n";
        }
        Debug.Log(allies);

        String enemies = "Enemy Units: \n";

        foreach (MGUnits.Job job in _enemyUnits.Keys)
        {
            enemies += $"{job}: {_enemyUnits[job]}\n";
        }
        Debug.Log(enemies);
    }
}