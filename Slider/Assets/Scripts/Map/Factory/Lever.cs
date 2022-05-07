using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : ElectricalNode
{
    [SerializeField]
    private GameObject offSprite;
    [SerializeField]
    private GameObject onSprite;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.INPUT;
    }

    private void Start()
    {
        OnSwitch(true);
    }

    public void OnSwitch(bool value)
    {
        offSprite.SetActive(!value);
        onSprite.SetActive(value);

        StartSignal(value);
    }
}
