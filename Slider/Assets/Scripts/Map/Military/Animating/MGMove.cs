using UnityEngine;

public class MGMove : IMGAnimatable
{
    public Vector2Int startCoords;
    public Vector2Int endCoords;
    public STile startStile;
    public STile endStile;

    public MGMove(
        MilitaryUnit unit, 
        Vector2Int startCoords, 
        Vector2Int endCoords, 
        STile startStile, 
        STile endStile
    ) {
        this.unit = unit;
        this.startCoords = startCoords;
        this.endCoords = endCoords;
        this.startStile = startStile;
        this.endStile = endStile;
    }

    public override void Execute(System.Action finishedCallback)
    {
        unit.NPCController.AnimateMove(this, false, finishedCallback);
    }
}