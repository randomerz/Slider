using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : ConductiveElectricalNode
{
    [SerializeField]
    private GameObject offSprite;
    [SerializeField]
    private GameObject onSprite;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.IO;
    }

    public override void OnPoweredHandler(bool value, bool valueChanged)
    {
        offSprite.SetActive(!value);
        onSprite.SetActive(value);
    }
}
