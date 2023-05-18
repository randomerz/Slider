using System;
using System.Collections.Generic;
using UnityEngine;

public class MGSpace
{
    private Dictionary<MGUnits.Job, int> _alliedUnits;   //EntityData, Qty
    private Dictionary<MGUnits.Job, int> _enemyUnits;   //EntityData, Qty

    public delegate void _OnSupplyDropSpawn();
    public event _OnSupplyDropSpawn OnSupplyDropSpawn;

    public MGSpace()
    {
        _alliedUnits = new();
        _enemyUnits = new();
    }

    public void AddUnits(MGUnits.Job job, MGUnits.Allegiance allegiance, int quantity)
    {
        if (quantity <= 0)
        {
            return;
        }

        Dictionary<MGUnits.Job, int> units;
        switch (allegiance)
        {
            case MGUnits.Allegiance.Ally:
                units = _alliedUnits;
                break;
            case MGUnits.Allegiance.Enemy:
            default:
                units = _enemyUnits;
                break;
        }

        if (units.ContainsKey(job))
        {
            units[job] += quantity;
        }
        else
        {
            units[job] = quantity;
        }
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