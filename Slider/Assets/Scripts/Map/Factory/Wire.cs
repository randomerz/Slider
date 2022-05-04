using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : ElectricalConductor
{
    [SerializeField]
    private GameObject offSprite;
    [SerializeField]
    private GameObject onSprite;

    public override void OnPoweredHandler(bool value)
    {
        offSprite.SetActive(!value);
        onSprite.SetActive(value);
    }
}
