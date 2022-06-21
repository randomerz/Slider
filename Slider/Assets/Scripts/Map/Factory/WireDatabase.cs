using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

[System.Serializable]
public struct WireTile
{
    public TileBase offWire;
    public TileBase onWire;
    public TileBase conductingAlt;
}

//The sparks are based on sprite instead of tile bc of rule tiles.
[System.Serializable]
struct SparkPair
{
    public Sprite wireSprite;
    [FormerlySerializedAs("sparkResName")]
    public string sparkType;
}

public class WireDatabase : MonoBehaviour
{
    public List<WireTile> wireTiles;

    [SerializeField]
    private List<SparkPair> sparks;

    private Dictionary<Sprite, string> _sparkTypes;
    public Dictionary<Sprite, string> Sparks => _sparkTypes;

    public Dictionary<TileBase, TileBase> ConductingAltOn { get; private set; }
    public Dictionary<TileBase, TileBase> ConductingAltOff { get; private set; }




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
        Debug.Log("Instance Created");
        _sparkTypes = new Dictionary<Sprite, string>();
        foreach (SparkPair pair in sparks)
        {
            _sparkTypes[pair.wireSprite] = pair.sparkType;
        }

        ConductingAltOn = new Dictionary<TileBase, TileBase>();
        ConductingAltOff = new Dictionary<TileBase, TileBase>();
        foreach (WireTile wire in wireTiles)
        {
            if (wire.onWire != null && wire.conductingAlt != null)
            {
                ConductingAltOn[wire.onWire] = wire.conductingAlt;
                ConductingAltOff[wire.conductingAlt] = wire.onWire;
            }
        }

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

