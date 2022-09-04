using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MinecartPowerNode : ElectricalNode
{
    [SerializeField] private Vector3Int loc;
    private Tilemap rm;
    private RailTile tile;

    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.OUTPUT;
        rm = GetComponentInParent<STileTilemap>().minecartRails;
        tile = rm.GetTile(loc) as RailTile;
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        base.OnPoweredHandler(e);
        tile.isPowered = !tile.isPowered;
    }
}
