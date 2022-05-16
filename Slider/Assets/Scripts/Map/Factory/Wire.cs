using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Wire : ConductiveElectricalNode
{
    private Tilemap tm;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.IO;

        tm = GetComponent<Tilemap>();
        SetTiles(Powered);
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        SetTiles(e.powered);
    }

    private void SetTiles(bool powered)
    {
        WireDatabase.Instance.SwapTiles(tm, powered);
    }
}
