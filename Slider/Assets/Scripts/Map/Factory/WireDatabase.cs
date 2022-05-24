using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public struct WireTile
{
    public TileBase offWire;
    public TileBase onWire;
}

public class WireDatabase : MonoBehaviour
{
    [SerializeField]
    private List<WireTile> wireTiles;


    private static WireDatabase _instance;

    public static WireDatabase Instance
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    private void OnDisable()
    {
        _instance = null;
    }

    public bool Contains(TileBase tile)
    {
        foreach (WireTile wire in wireTiles)
        {
            if (wire.offWire.Equals(tile) || wire.onWire.Equals(tile))
            {
                return true;
            }
        }

        return false;
    }

    public void SwapTiles(Tilemap tm, bool powered)
    {
        foreach (WireTile wire in wireTiles)
        {

            tm.SwapTile(powered ? wire.offWire : wire.onWire, powered ? wire.onWire : wire.offWire);            
        }
    }
}

