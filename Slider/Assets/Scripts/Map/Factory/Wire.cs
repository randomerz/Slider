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
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => WireDatabase.Instance != null);
        SetTiles();
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        SetTiles();
    }

    private void SetTiles()
    {
        WireDatabase.Instance.SwapTiles(tm, Powered);
    }
}
