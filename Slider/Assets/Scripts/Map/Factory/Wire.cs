using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Wire : ConductiveElectricalNode
{
    [Header("Wire")]
    [SerializeField] private Tilemap wireTilemap;
    [SerializeField] private Tilemap dottedWireTilemap;
    [SerializeField] private bool logTrace = false;

    private GameObject sparks;
    private List<GameObject> sparkInstances;



    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.IO;

        sparks = Resources.Load<GameObject>("WireSparks");
        sparkInstances = new List<GameObject>();
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => WireDatabase.Instance != null);
        SetTiles();
    }

    public override void OnPoweredHandler(OnPoweredArgs e)
    {
        LogTrace("OnPoweredHandler was called: " + e.powered);
        base.OnPoweredHandler(e);
        SetTiles();

        if (e.powered)
        {
            CreateSparks();
        } else
        {
            foreach (GameObject go in sparkInstances)
            {
                Destroy(go);
            }
            sparkInstances.Clear();
        }
  
    }

    private void CreateSparks()
    {
        foreach (Vector3Int pos in wireTilemap.cellBounds.allPositionsWithin)
        {
            CreateSparksForTile(wireTilemap, pos);
        }
    }

    private void CreateSparksForTile(Tilemap tm, Vector3Int pos)
    {
        if (tm.GetTile(pos) != null)
        {
            if (WireDatabase.Instance.Sparks.ContainsKey(tm.GetSprite(pos)))
            {
                GameObject sparkInstance = Instantiate(sparks, tm.CellToWorld(pos) + tm.tileAnchor, Quaternion.identity, transform);
                sparkInstance.GetComponent<WireSparks>().StartSparks(WireDatabase.Instance.Sparks[tm.GetSprite(pos)]);

                SpriteRenderer sr = sparkInstance.GetComponent<SpriteRenderer>();
                TilemapRenderer tr = wireTilemap.GetComponent<TilemapRenderer>();
                sr.sortingLayerID = tr.sortingLayerID;
                sr.sortingOrder = tr.sortingOrder;

                sparkInstances.Add(sparkInstance);
            }
            else
            {
                //Debug.LogWarning($"Sparks not found for tile at position {pos} with sprite {tm.GetSprite(pos)}");
            }

        }
    }

    private void SetTiles()
    {
        WireDatabase.Instance.SwapTiles(wireTilemap, Powered);
        WireDatabase.Instance.SwapTiles(dottedWireTilemap, Powered);
    }

    private void LogTrace(string s)
    {
        if (logTrace)
        {
            Debug.Log($"[{name}] {s}");
        }
    }
}
