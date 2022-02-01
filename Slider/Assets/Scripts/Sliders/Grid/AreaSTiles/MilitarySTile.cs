using UnityEngine;

public class MilitarySTile : STile 
{
    // public new int STILE_WIDTH = 13;

    protected new void Awake() {
        STILE_WIDTH = 13;

        base.Awake();
    }
}