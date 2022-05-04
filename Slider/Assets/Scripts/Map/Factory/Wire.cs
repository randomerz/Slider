using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : ElectricalNode
{
    [SerializeField]
    private GameObject offSprite;
    [SerializeField]
    private GameObject onSprite;

    private void Awake()
    {
        nodeType = NodeType.IO;
    }

    public override void OnPoweredHandler(bool value, bool valueChanged)
    {
        offSprite.SetActive(!value);
        onSprite.SetActive(value);
    }
}
