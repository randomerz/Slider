using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsJungleBox : JungleBox
{
    protected override void Awake()
    {
    }

    public override bool AddInput(JungleBox other, Direction fromDirection)
    {
        return true;
    }

    public override bool IsValidInput(JungleBox other, Direction fromDirection)
    {
        return true;
    }

    public override void RemoveInput(Direction fromDirection)
    {
       
    }

    public override bool UpdateBox(int depth = 0)
    {
        return true;
    }

}
