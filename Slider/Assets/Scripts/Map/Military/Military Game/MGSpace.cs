using System;
using System.Collections.Generic;
using UnityEngine;

public class MGSpace
{
    private Dictionary<MGUnits.Job, int> _alliedUnits;   //EntityData, Qty
    private Dictionary<MGUnits.Job, int> _enemyUnits;

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

        int existingUnits;
        if (units.TryGetValue(job, out existingUnits))
        {
            units[job] += quantity;
        }
        else
        {
            units[job] = quantity;
        }
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