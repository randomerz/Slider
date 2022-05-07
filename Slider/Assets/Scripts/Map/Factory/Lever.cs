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
    }

    public void OnSwitch()
    {
        StartSignal(!Powered);
        offSprite.SetActive(!Powered);
        onSprite.SetActive(Powered);
    }
}
