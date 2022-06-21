using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Wire : ConductiveElectricalNode
{
    private Tilemap tm;
    private GameObject sparks;
    private List<GameObject> sparkInstances;



    private new void Awake()
    {
        base.Awake();
        nodeType = NodeType.IO;

        tm = GetComponent<Tilemap>();
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
        base.OnPoweredHandler(e);
        SetTiles();

        if (e.powered)
        {
            //Do Sparks
            foreach (Vector3Int pos in tm.cellBounds.allPositionsWithin)
            {

                if (tm.GetTile(pos) != null)
                {
                    if (WireDatabase.Instance.Sparks.ContainsKey(tm.GetSprite(pos)))
                    {
                        GameObject sparkInstance = Instantiate(sparks, tm.CellToWorld(pos) + tm.tileAnchor, Quaternion.identity, transform);
                        sparkInstance.GetComponent<WireSparks>().StartSparks(WireDatabase.Instance.Sparks[tm.GetSprite(pos)]);

                        SpriteRenderer sr = sparkInstance.GetComponent<SpriteRenderer>();
                        TilemapRenderer tr = GetComponent<TilemapRenderer>();
                        sr.sortingLayerID = tr.sortingLayerID;
                        sr.sortingOrder = tr.sortingOrder;

                        sparkInstances.Add(sparkInstance);
                    } else
                    {
                        Debug.LogError($"Sparks not found for tile at position {pos} with sprite {tm.GetSprite(pos)}");
                    }

                }
            }
        } else
        {
            foreach (GameObject go in sparkInstances)
            {
                Destroy(go);
            }
            sparkInstances.Clear();
        }
  
    }

    private void SetTiles()
    {
        WireDatabase.Instance.SwapTiles(tm, Powered);
    }
}
