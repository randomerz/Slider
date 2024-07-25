using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a strategy to be used by the ""AI"" military commander when commanding their units.
/// </summary>
public interface ICommanderStrategy
{
    /// <summary>
    /// Issues a command to each of the passed in units, telling them how to take their turn.
    /// </summary>
    public void PerformTurn(List<MilitaryUnit> unitsToCommand);
}
