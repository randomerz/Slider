using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilitaryUnitCommander : MonoBehaviour
{
    private readonly ICommanderStrategy strategy = new MoveTowardsStrategy();

    private readonly List<MilitaryUnit> units = new();

    private void Start()
    {
        // // This is for debugging purposes
        // Controls.RegisterBindingBehavior(this, Controls.Bindings.Player.AltViewHold, (_) => PerformTurn());
    }

    public void PerformTurn()
    {
        strategy.PerformTurn(units);
    }

    /// <summary>
    /// Adds the passed in unit to this commander, meaning it will receive orders from this commander on the commander's turn.
    /// Make sure to remove the unit using <see cref="RemoveUnit(MilitaryUnit)"/> when the unit is no longer alive.
    /// </summary>
    /// <param name="unit"></param>
    public void AddUnit(MilitaryUnit unit)
    {
        units.Add(unit);
    }

    /// <summary>
    /// Remove the passed in unit from this commander, meaning it will no longer receive orders from this commander on the commander's turn.
    /// </summary>
    /// <param name="unit"></param>
    public void RemoveUnit(MilitaryUnit unit)
    {
        units.Remove(unit);
    }
}
