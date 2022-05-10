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

        //Just initializing to make sure they're not both active at the same time
        offSprite.SetActive(!Powered);
        onSprite.SetActive(Powered);
    }

    public void OnPoweredHandler(OnPoweredArgs args)
    {
        offSprite.SetActive(!args.powered);
        onSprite.SetActive(args.powered);
    }
}
