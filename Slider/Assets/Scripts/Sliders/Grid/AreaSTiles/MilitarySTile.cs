using UnityEngine;

public class MilitarySTile : STile 
{
    public Transform sliderCollectibleSpawn;
    public Transform newUnitSelectorSpawn;

    private new void Awake() {
        base.Awake();
        // STILE_WIDTH = 13;
    }
}