using UnityEngine;

public class MilitarySTile : STile 
{
    // public new int STILE_WIDTH = 13;

    private new void Awake() {
        base.Awake();
        STILE_WIDTH = 13;
    }
}